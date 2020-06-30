using Api.Extensions;
using Api.Persistence;
using MediatR;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Domain.Queries
{
    public class ListMyGamesQuery : IRequest<IEnumerable<Player.Game>>
    {
        [JsonConstructor]
        public ListMyGamesQuery()
        {
        }

        public class ListMyGamesQueryHandler : IRequestHandler<ListMyGamesQuery, IEnumerable<Player.Game>>
        {
            private readonly PlayerIdentity _playerIdentity;
            private readonly IGameRepository _repository;

            public ListMyGamesQueryHandler(PlayerIdentity playerIdentity, IGameRepository repository)
            {
                _playerIdentity = playerIdentity;
                _repository = repository;
            }

            public async Task<IEnumerable<Player.Game>> Handle(ListMyGamesQuery request, CancellationToken cancellationToken)
            {
                var games = await _repository.GetPlayerGamesAsync(_playerIdentity.Id);
                return games;
            }
        }
    }
}
