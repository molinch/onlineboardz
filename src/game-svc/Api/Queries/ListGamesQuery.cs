using Api.Extensions;
using Api.Persistence;
using MediatR;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Commands
{
    public class ListGamesQuery : IRequest<IEnumerable<Game>>
    {
        [JsonConstructor]
        public ListGamesQuery(IEnumerable<GameType> types, IEnumerable<GameStatus> statuses)
        {
            Types = types;
            Statuses = statuses;
        }

        public IEnumerable<GameType> Types { get; }
        public IEnumerable<GameStatus> Statuses { get; }

        public class ListGamesQueryHandler : IRequestHandler<ListGamesQuery, IEnumerable<Game>>
        {
            private readonly PlayerIdentity _playerIdentity;
            private readonly IGameRepository _repository;

            public ListGamesQueryHandler(PlayerIdentity playerIdentity, IGameRepository repository)
            {
                _playerIdentity = playerIdentity;
                _repository = repository;
            }

            public Task<IEnumerable<Game>> Handle(ListGamesQuery request, CancellationToken cancellationToken)
            {
                return _repository.GetAsync(_playerIdentity.Id, request.Types, request.Statuses);
            }
        }
    }
}
