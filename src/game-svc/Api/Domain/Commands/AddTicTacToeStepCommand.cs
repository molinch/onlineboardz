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

namespace Api.Domain.Commands
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

                var nextPlayer = game.NextPlayer;
                if (nextPlayer.ID != _playerIdentity.Id)
                {
                    throw new ValidationException($"It is not your turn to play, current player is {nextPlayer.ID}");
                }

                var emptyCellsCount = game.EmptyCellsCount;
                var stepNumber = TicTacToe.CellCount - emptyCellsCount;
                game.Cells[request.CellIndex] = new TicTacToe.CellData() { Number = stepNumber };
                var won = game.HasWon();

                game.Status = won || (emptyCellsCount == 1) // this will complete the game
                    ? GameStatus.Finished
                    : GameStatus.InGame;

                if (game.Status != GameStatus.InGame)
                {
                    foreach (var player in game.Players)
                    {
                        bool isMe = player.ID == _playerIdentity.Id;
                        player.Status = (won, isMe) switch {
                            (true, true)  => player.Status = PlayerGameStatus.Won,
                            (true, false) => player.Status = PlayerGameStatus.Lost,
                            _ => PlayerGameStatus.Draw
                        };
                    }
                }

                await _ticTacToeRepository.SetTicTacToeStepAsync(game, request.CellIndex);

                // remark: we should probably use a transaction as we update both Collections
                if (game.Status != GameStatus.InGame)
                {
                    foreach (var player in game.Players)
                    {
                        // since game status changed we want to reflect that in Player collection
                        await _gameRepository.AddOrUpdatePlayerGameAsync(player.ID, Player.Game.From(player.ID, game));
                    }
                }

                await _gameHub.Clients.Users(game.Players.Select(p => p.ID))
                        .SendAsync("GameStepAdded", game);

                return game;
            }
        }
    }
}
