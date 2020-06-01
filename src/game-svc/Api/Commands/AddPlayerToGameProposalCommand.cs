using Api.Exceptions;
using Api.Extensions;
using Api.Persistence;
using MediatR;
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

            public AddPlayerToGameProposalCommandHandler(PlayerIdentity playerIdentity, IGameRepository repository)
            {
                _playerIdentity = playerIdentity;
                _repository = repository;
            }

            public async Task<Game> Handle(AddPlayerToGameProposalCommand request, CancellationToken cancellationToken)
            {
                if (await _repository.IsAlreadyInGamesAsync(_playerIdentity.Id))
                {
                    throw new ValidationException("You are already in a game");
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
                    waitingRoom = await _repository.SetGameStatus(waitingRoom.ID, waitingRoom.Status, GameStatus.InGame);
                    // inform that game started via SignalR
                }

                return waitingRoom;
            }
        }
    }
}
