using Api.Extensions;
using Api.Persistence;
using MediatR;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Domain.Queries
{
    public class ListPlayableGamesQuery : IRequest<IEnumerable<Game>>
    {
        [JsonConstructor]
        public ListPlayableGamesQuery(IEnumerable<GameType> types, IEnumerable<GameStatus> statuses)
        {
            Types = types;
            Statuses = statuses;
        }

        public IEnumerable<GameType> Types { get; }
        public IEnumerable<GameStatus> Statuses { get; }

        public class ListGamesQueryHandler : IRequestHandler<ListPlayableGamesQuery, IEnumerable<Game>>
        {
            private readonly PlayerIdentity _playerIdentity;
            private readonly IGameRepository _repository;

            public ListGamesQueryHandler(PlayerIdentity playerIdentity, IGameRepository repository)
            {
                _playerIdentity = playerIdentity;
                _repository = repository;
            }

            public async Task<IEnumerable<Game>> Handle(ListPlayableGamesQuery request, CancellationToken cancellationToken)
            {
                var games = await _repository.GetPlayableGamesAsync(_playerIdentity.Id, request.Types, request.Statuses);
                return games;
            }
        }
    }
}
