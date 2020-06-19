using Api.Domain;
using Api.Exceptions;
using Api.Extensions;
using Api.Persistence;
using Api.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Commands
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
            private readonly IHubContext<GameHub> _gameHub;
            private readonly GameAssert _gameAssert;
            private readonly IUniqueRandomRangeCreator _uniqueRandomRangeCreator;

            public AddPlayerToGameProposalCommandHandler(PlayerIdentity playerIdentity, IGameRepository repository,
                IHubContext<GameHub> gameHub, GameAssert gameAssert, IUniqueRandomRangeCreator uniqueRandomRangeCreator)
            {
                _playerIdentity = playerIdentity;
                _repository = repository;
                _gameHub = gameHub;
                _gameAssert = gameAssert;
                _uniqueRandomRangeCreator = uniqueRandomRangeCreator;
            }

            public async Task<Game> Handle(AddPlayerToSpecificGameCommand request, CancellationToken cancellationToken)
            {
                await _gameAssert.NotTooManyOpenGamesAsync();

                var waitingRoom = await _repository.AddPlayerIfNotThereAsync(request.GameId, new Game.Player()
                {
                    ID = _playerIdentity.Id,
                    Name = _playerIdentity.Name,
                    AcceptedAt = DateTime.UtcNow
                });

                if (waitingRoom == null)
                {
                    waitingRoom = await _repository.GetAsync(request.GameId);
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

                if (waitingRoom.MaxPlayers == waitingRoom.Players.Count)
                {
                    var playerOrders = _uniqueRandomRangeCreator.CreateArrayWithAllNumbersFromRange(waitingRoom.Players.Count);
                    var game = await _repository.StartGameAsync(waitingRoom.ID!, playerOrders);
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
