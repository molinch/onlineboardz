using Api.Exceptions;
using Api.Extensions;
using Api.Persistence;
using Api.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Commands
{
    public class AddPlayerToGameProposalCommand : IRequest<Game>
    {
        [JsonConstructor]
        public AddPlayerToGameProposalCommand(string gameId)
        {
            GameId = gameId;
        }

        public string GameId { get; }

        public class AddPlayerToGameProposalCommandHandler : IRequestHandler<AddPlayerToGameProposalCommand, Game>
        {
            private readonly PlayerIdentity _playerIdentity;
            private readonly IGameRepository _repository;
            private readonly IHubContext<GameHub> _gameHub;
            private readonly IOptions<GameOptions> _gameOptions;

            public AddPlayerToGameProposalCommandHandler(PlayerIdentity playerIdentity, IGameRepository repository,
                IHubContext<GameHub> gameHub, IOptions<GameOptions> gameOptions)
            {
                _playerIdentity = playerIdentity;
                _repository = repository;
                _gameHub = gameHub;
                _gameOptions = gameOptions;
            }

            public async Task<Game> Handle(AddPlayerToGameProposalCommand request, CancellationToken cancellationToken)
            {
                if ((await _repository.GetNumberOfGamesAsync(_playerIdentity.Id)) >= _gameOptions.Value.MaxNumberOfGamesPerUser)
                {
                    throw new ValidationException("Maximum number of games reached");
                }

                var waitingRoom = await _repository.AddPlayerIfNotThereAsync(request.GameId, new Game.GameMetadata.Player()
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

                    if (waitingRoom.Status != GameStatus.WaitingForPlayers ||
                        waitingRoom.Metadata.MaxPlayers == waitingRoom.Metadata.PlayersCount)
                    {
                        throw new ValidationException("Maximum number of players already reached");
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
