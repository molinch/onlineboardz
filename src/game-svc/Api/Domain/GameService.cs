using Api.Extensions;
using Api.Persistence;
using Api.SignalR;
using System.Threading.Tasks;

namespace Api.Domain
{
    public class GameService
    {
        private readonly PlayerIdentity _playerIdentity;
        private readonly IGameRepository _repository;
        private readonly IGameHubSender _gameHub;
        private readonly IUniqueRandomRangeCreator _uniqueRandomRangeCreator;

        public GameService(PlayerIdentity playerIdentity, IGameRepository repository,
                IGameHubSender gameHub, IUniqueRandomRangeCreator uniqueRandomRangeCreator)
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
                var game = await _repository.StartGameAsync(passedGame.Id!, playerOrders);

                if (game != null)
                {
                    foreach (var player in game.Players)
                    {
                        await _repository.AddOrUpdatePlayerGameAsync(player.Id, Player.Game.From(player.Id, game));
                    }

                    await _gameHub.GameStartedAsync(game);

                    return game;
                }
            }
            else
            {
                await _gameHub.PlayerAddedAsync(passedGame);
            }

            return passedGame;
        }
    }
}
