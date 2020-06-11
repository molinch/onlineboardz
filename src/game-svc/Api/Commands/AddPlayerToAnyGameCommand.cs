using Api.Exceptions;
using Api.Extensions;
using Api.Persistence;
using Api.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Commands
{
    public class AddPlayerToAnyGameCommand : IRequest<Game>
    {
        [JsonConstructor]
        public AddPlayerToAnyGameCommand(GameType gameType)
        {
            GameType = gameType;
        }

        public GameType GameType { get; }

        public class AddPlayerToAnyGameCommandHander : IRequestHandler<AddPlayerToAnyGameCommand, Game>
        {
            private readonly PlayerIdentity _playerIdentity;
            private readonly IGameRepository _repository;
            private readonly GameAssert _gameAssert;
            private readonly IHubContext<GameHub> _gameHub;

            public AddPlayerToAnyGameCommandHander(PlayerIdentity playerIdentity, IGameRepository repository,
                IHubContext<GameHub> gameHub, GameAssert gameAssert)
            {
                _playerIdentity = playerIdentity;
                _repository = repository;
                _gameHub = gameHub;
                _gameAssert = gameAssert;
            }

            public async Task<Game> Handle(AddPlayerToAnyGameCommand request, CancellationToken cancellationToken)
            {
                await _gameAssert.NotTooManyOpenGamesAsync();

                var waitingRoom = await _repository.AddPlayerToGameAsync(request.GameType, new Game.GameMetadata.Player()
                {
                    ID = _playerIdentity.Id,
                    Name = _playerIdentity.Name,
                    AcceptedAt = DateTime.UtcNow
                });

                if (waitingRoom == null)
                {
                    if (waitingRoom == null)
                    {
                        throw new ValidationException($"There are no '{request.GameType}' games waiting for player");
                    }
                }

                if (waitingRoom.Metadata.MaxPlayers == waitingRoom.Metadata.Players.Count)
                {
                    var game = await _repository.SetGameStatusAsync(waitingRoom.ID, waitingRoom.Status, GameStatus.InGame);
                    await _gameHub.Clients.All.SendAsync("GameStarted", game);
                }
                else
                {
                    await _gameHub.Clients.All.SendAsync("PlayerAdded", waitingRoom);
                }

                return waitingRoom;
            }
        }
    }
}
