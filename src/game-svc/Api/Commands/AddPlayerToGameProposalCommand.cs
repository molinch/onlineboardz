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
            private readonly IGameRepository _waitingRoomRepository;

            public AddPlayerToGameProposalCommandHandler(PlayerIdentity playerIdentity, IGameRepository repository)
            {
                _playerIdentity = playerIdentity;
                _waitingRoomRepository = repository;
            }

            public async Task<Game> Handle(AddPlayerToGameProposalCommand request, CancellationToken cancellationToken)
            {
                if (await _waitingRoomRepository.IsAlreadyInWaitingRoomAsync(_playerIdentity.Id))
                {
                    throw new Exception("You are already in a waiting room");
                }

                var game = await _waitingRoomRepository.AddPlayerIfNotThereAsync(request.GameId, new Game.GameMetadata.Player()
                {
                    Id = _playerIdentity.Id,
                    Name = _playerIdentity.Name,
                    AcceptedAt = DateTime.UtcNow
                });

                if (game.Metadata.MaxPlayers == game.Metadata.Players.Count)
                {
                    game.Status = GameStatus.InGame;
                    // notify via signalR
                }

                return game;
            }
        }
    }
}
