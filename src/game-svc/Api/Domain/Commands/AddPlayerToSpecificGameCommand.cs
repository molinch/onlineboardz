using Api.Exceptions;
using Api.Extensions;
using Api.Persistence;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Domain.Commands
{
    public class AddPlayerToSpecificGameCommand : IRequest<Game>
    {
        [JsonConstructor]
        public AddPlayerToSpecificGameCommand(string gameId)
        {
            GameId = gameId;
        }

        public string GameId { get; }

        public class AddPlayerToGameProposalCommandHandler : IRequestHandler<AddPlayerToSpecificGameCommand, Game>
        {
            private readonly PlayerIdentity _playerIdentity;
            private readonly IGameRepository _repository;
            private readonly GameAssert _gameAssert;
            private readonly GameService _gameService;

            public AddPlayerToGameProposalCommandHandler(PlayerIdentity playerIdentity, IGameRepository repository,
                GameAssert gameAssert, GameService gameService)
            {
                _playerIdentity = playerIdentity;
                _repository = repository;
                _gameAssert = gameAssert;
                _gameService = gameService;
            }

            public async Task<Game> Handle(AddPlayerToSpecificGameCommand request, CancellationToken cancellationToken)
            {
                await _repository.CreatePlayerIfNotThereAsync(new Player()
                {
                    ID = _playerIdentity.Id,
                    Name = _playerIdentity.Name,
                });

                await _gameAssert.NotTooManyOpenGamesAsync();

                var waitingRoom = await _repository.AddPlayerIfNotThereAsync(request.GameId, new Game.Player()
                {
                    ID = _playerIdentity.Id,
                    Name = _playerIdentity.Name,
                    AcceptedAt = DateTime.UtcNow
                });

                if (waitingRoom == null)
                {
                    waitingRoom = await _repository.GetAsync<TicTacToe>(request.GameId);
                    if (waitingRoom == null)
                    {
                        throw new ItemNotFoundException();
                    }
                    
                    if (waitingRoom.Players.Any(p => p.ID == _playerIdentity.Id))
                    {
                        throw new ValidationException("You are already in the game");
                    }

                    if (waitingRoom.Status != GameStatus.WaitingForPlayers ||
                        waitingRoom.MaxPlayers == waitingRoom.PlayersCount)
                    {
                        throw new ValidationException("Maximum number of players already reached");
                    }
                }

                return await _gameService.JoinAsync(waitingRoom);
            }
        }
    }
}
