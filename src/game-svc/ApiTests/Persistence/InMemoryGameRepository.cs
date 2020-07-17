using Api.Domain;
using Api.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiTests.Persistence
{
    public class InMemoryGameRepository : IGameRepository
    {
        private readonly InMemoryDb _db;

        public InMemoryGameRepository(InMemoryDb db)
        {
            _db = db;
        }
        private Dictionary<string, Player> Players => _db.Players;
        private Dictionary<string, Game> Games => _db.Games;

        public Task AddOrUpdatePlayerGameAsync(string playerId, Player.Game game)
        {
            if (Players.TryGetValue(playerId, out var player))
            {
                player.Games.RemoveAll(g => g.Id == game.Id);
                player.Games.Add(game);
            }

            return Task.CompletedTask;
        }

        public Task<Game?> AddPlayerToGameIfNotThereAsync(string gameId, Game.Player player)
        {
            if (Games.TryGetValue(gameId, out var game))
            {
                game.Players.RemoveAll(p => p.Id == player.Id);
                game.Players.Add(player);
            }

            return Task.FromResult(game);
        }

        public Task<Game?> AddPlayerToGameAsync(GameType gameType, int? maxPlayers, int? duration, Game.Player player)
        {
            var game = Games.Select(kv => kv.Value).FirstOrDefault(g => g.GameType == gameType
                && (maxPlayers == null || g.MaxPlayers <= maxPlayers)
                && (duration == null || g.MaxDuration <= duration)
                && !g.Players.Any(p => p.Id == player.Id));

            if (game != null)
            {
                game.Players.Add(player);
            }

            return Task.FromResult(game);
        }

        public Task<TGame> CreateGameAsync<TGame>(TGame game) where TGame : Game
        {
            game.Id = Guid.NewGuid().ToString();
            Games.Add(game.Id!, game);
            return Task.FromResult(game);
        }

        public Task<bool> CreatePlayerIfNotExistingAsync(Player player)
        {
            if (Players.ContainsKey(player.Id)) return Task.FromResult(false);
            Players.Add(player.Id, player);
            return Task.FromResult(true);
        }

        public Task<TGame?> GetAsync<TGame>(string gameId) where TGame : Game
        {
            var game = Games.Select(g => g.Value).OfType<TGame>().FirstOrDefault(g => g.Id == gameId);
            return Task.FromResult(game);
        }

        public Task<int> GetNumberOfGamesAsync(string playerId)
        {
            if (!Players.TryGetValue(playerId, out var player))
                return Task.FromResult(0);

            return Task.FromResult(player.Games.Count);
        }

        public Task<IEnumerable<Game>> GetPlayableGamesAsync(string playerId, IEnumerable<GameType> gameTypes, IEnumerable<GameStatus> statuses)
        {
            var games = Games.Select(g => g.Value)
                .Where(g => !g.Players.Any(p => p.Id == playerId))
                .Where(g => gameTypes.Contains(g.GameType) && statuses.Contains(g.Status));
            return Task.FromResult(games);
        }

        public Task<IEnumerable<Player.Game>> GetPlayerGamesAsync(string playerId)
        {
            if (!Players.TryGetValue(playerId, out var player))
                return Task.FromResult(Enumerable.Empty<Player.Game>());

            return Task.FromResult(player.Games.AsEnumerable());
        }

        public Task<IEnumerable<Player.Game>> GetPlayerGamesAsync(string playerId, GameStatus? status)
        {
            if (!Players.TryGetValue(playerId, out var player))
                return Task.FromResult(Enumerable.Empty<Player.Game>());

            return Task.FromResult(player.Games.Where(g => status == null || g.Status == status.Value));
        }

        public Task RemoveAsync(string gameId)
        {
            Games.Remove(gameId);
            return Task.CompletedTask;
        }

        public Task<Game?> StartGameAsync(string gameId, IReadOnlyList<int> playerOrders)
        {
            if (Games.TryGetValue(gameId, out var game))
            {
                game.Status = GameStatus.InGame;
                for (var i = 0; i < playerOrders.Count; i++)
                {
                    game.Players[i].PlayOrder = playerOrders[i];
                }
            }

            return Task.FromResult(game);
        }
    }
}
