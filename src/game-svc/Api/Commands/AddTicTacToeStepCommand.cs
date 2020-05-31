using Api.Extensions;
using Api.Persistence;
using MediatR;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Commands
{
    public class AddTicTacToeStepCommand : IRequest<Game>
    {
        [JsonConstructor]
        public AddTicTacToeStepCommand(string gameId)
        {
            GameId = gameId;
        }

        public string GameId { get; }

        public class AddTicTacToeStepCommandHandler : IRequestHandler<AddTicTacToeStepCommand, Game>
        {
            private readonly PlayerIdentity _playerIdentity;
            private readonly IGameRepository _waitingRoomRepository;

            public AddTicTacToeStepCommandHandler(PlayerIdentity playerIdentity, IGameRepository repository)
            {
                _playerIdentity = playerIdentity;
                _waitingRoomRepository = repository;
            }

            public Task<Game> Handle(AddTicTacToeStepCommand request, CancellationToken cancellationToken)
            {
                // update game state
                // notify step by SignalR

                return null!;
            }
        }
    }
}
