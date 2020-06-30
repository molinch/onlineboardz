using Api.Extensions;
using Api.Persistence;
using Api.SignalR;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Domain
{
    public class GameService
    {
        private readonly PlayerIdentity _playerIdentity;
        private readonly IGameRepository _repository;
        private readonly IHubContext<GameHub> _gameHub;
        private readonly IUniqueRandomRangeCreator _uniqueRandomRangeCreator;

        public GameService(PlayerIdentity playerIdentity, IGameRepository repository,
                IHubContext<GameHub> gameHub, IUniqueRandomRangeCreator uniqueRandomRangeCreator)
        {
            _playerIdentity = playerIdentity;
            _repository = repository;
            _gameHub = gameHub;
            _uniqueRandomRangeCreator = uniqueRandomRangeCreator;
        }

        public async Task<Game> JoinAsync(Game passedGame)
        {
            await _repository.AddOrUpdatePlayerGameAsync(_playerIdentity.Id, Player.Game.From(_playerIdentity.Id, passedGame));

            if (passedGame.MaxPlayers == passedGame.Players.Count)
            {
                var playerOrders = _uniqueRandomRangeCreator.CreateArrayWithAllNumbersFromRange(passedGame.Players.Count);
                var game = await _repository.StartGameAsync(passedGame.ID!, playerOrders);

                if (game != null)
                {
                    foreach (var player in game.Players)
                    {
                        await _repository.AddOrUpdatePlayerGameAsync(player.ID, Player.Game.From(player.ID, game));
                    }

                    await _gameHub.Clients.Users(game.Players.Select(p => p.ID))
                        .SendAsync("GameStarted", game);

                    return game;
                }
            }
            else
            {
                await _gameHub.Clients.Users(passedGame.Players.Select(p => p.ID))
                    .SendAsync("PlayerAdded", passedGame);
            }

            return passedGame;
        }
    }
}
