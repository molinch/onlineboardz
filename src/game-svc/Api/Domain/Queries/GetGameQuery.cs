using Api.Exceptions;
using Api.Persistence;
using MediatR;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Domain.Queries
{
    public class GetGameQuery : IRequest<Game>
    {
        [JsonConstructor]
        public GetGameQuery(string gameId)
        {
            GameId = gameId;
        }

        public string GameId { get; }

        public class GetGameQueryHandler : IRequestHandler<GetGameQuery, Game>
        {
            private readonly IGameRepository _repository;

            public GetGameQueryHandler(IGameRepository repository)
            {
                _repository = repository;
            }

            public async Task<Game> Handle(GetGameQuery request, CancellationToken cancellationToken)
            {
                var game = await _repository.GetAsync<Game>(request.GameId);
                if (game == null)
                {
                    throw new ItemNotFoundException();
                }

                return game;
            }
        }
    }
}
