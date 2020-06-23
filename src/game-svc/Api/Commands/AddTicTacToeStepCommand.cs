using Api.Exceptions;
using Api.Extensions;
using Api.Persistence;
using Api.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Commands
{
    public class AddTicTacToeStepCommand : IRequest<Game>
    {
        [JsonConstructor]
        public AddTicTacToeStepCommand(string gameId, int cellIndex)
        {
            GameId = gameId;
            CellIndex = cellIndex;
        }

        public string GameId { get; }
        public int CellIndex { get; }

        public class AddTicTacToeStepCommandHandler : IRequestHandler<AddTicTacToeStepCommand, Game>
        {
            private readonly PlayerIdentity _playerIdentity;
            private readonly IGameRepository _gameRepository;
            private readonly ITicTacToeRepository _ticTacToeRepository;
            private readonly IHubContext<GameHub> _gameHub;

            public AddTicTacToeStepCommandHandler(PlayerIdentity playerIdentity, IGameRepository gameRepository, ITicTacToeRepository ticTacToeRepository, IHubContext<GameHub> gameHub)
            {
                _playerIdentity = playerIdentity;
                _gameRepository = gameRepository;
                _ticTacToeRepository = ticTacToeRepository;
                _gameHub = gameHub;
            }

            public async Task<Game> Handle(AddTicTacToeStepCommand request, CancellationToken cancellationToken)
            {
                var game = await _gameRepository.GetAsync<TicTacToe>(request.GameId);
                if (game == null)
                {
                    throw new ItemNotFoundException();
                }

                if (game.Status != GameStatus.InGame)
                {
                    throw new ValidationException("Game is not yet or no longer playable");
                }

                if (request.CellIndex < 0 || request.CellIndex >= TicTacToe.CellCount)
                {
                    throw new ValidationException($"Invalid cell index, it should be greather than zero and less than {TicTacToe.CellCount}");
                }

                if (game.Cells[request.CellIndex] != null)
                {
                    throw new ValidationException("This cell has already a value");
                }

                var nextPlayerId = game.NextPlayerId ?? game.Players.First(p => p.PlayOrder == 0).ID;
                if (nextPlayerId != _playerIdentity.Id)
                {
                    throw new ValidationException($"It is not your turn to play, current player is {nextPlayerId}");
                }

                int emptyCells = game.Cells.Where(g => g == null).Count();

                GameStatus newStatus = (emptyCells == 1) // this will complete the game
                    ? GameStatus.Finished
                    : GameStatus.InGame;

                var player = game.Players.First(p => p.ID == _playerIdentity.Id);
                var playerBoolean = player.PlayOrder == 0; // true is X, false is O
                var nextPlayerOrder = (player.PlayOrder + 1) % game.PlayersCount;
                var nextPlayer = game.Players.First(p => p.PlayOrder == nextPlayerOrder);
                var stepNumber = TicTacToe.CellCount - emptyCells;
                game = await _ticTacToeRepository.SetTicTacToeStepAsync(nextPlayer.ID, request.GameId, request.CellIndex, playerBoolean, stepNumber, newStatus);

                await _gameHub.Clients.Users(game.Players.Select(p => p.ID))
                        .SendAsync("GameStepAdded", game);

                return game;
            }
        }
    }
}
