using Api.Domain;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Threading.Tasks;

namespace Api.SignalR
{
    public class GameHubSender : IGameHubSender
    {
        private readonly IMapper _mapper;
        private readonly IHubContext<GameHub> _gameHub;

        public GameHubSender(IMapper mapper, IHubContext<GameHub> gameHub)
        {
            _mapper = mapper;
            _gameHub = gameHub;
        }

        public Task PlayerAddedAsync(Game game)
        {
            return CustomAsync<Game, TransferObjects.Game>("PlayerAdded", game);
        }

        public Task GameStartedAsync(Game game)
        {
            return CustomAsync<Game, TransferObjects.Game>("GameStarted", game);
        }

        public Task CustomAsync<TIn, TOut>(string method, TIn game)
            where TIn : Game
            where TOut : TransferObjects.Game
        {
            var gameOut = _mapper.Map<TOut>(game);
            return _gameHub.Clients.Users(game.Players.Select(p => p.ID))
                    .SendAsync(method, gameOut);
        }
    }
}
