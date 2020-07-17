using Api.Domain;
using Api.Persistence;
using System;
using System.Collections.Concurrent;
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
        private ConcurrentDictionary<string, Player> Players => _db.Players;
        private ConcurrentDictionary<string, Game> Games => _db.Games;

        public Task AddOrUpdatePlayerGameAsync(string playerId, Player.Game game)
        {
            if (Players.TryGetValue(playerId, out var player))
            {
                player.Games.RemoveAll(g => g.Id == game.Id);
                player.Games.Add(game.Clone());
            }

            if (player == null) throw new UpdateException();

            return Task.CompletedTask;
        }

        public Task<Game?> AddPlayerToGameIfNotThereAsync(string gameId, Game.Player player)
        {
            var game = Games.Select(g => g.Value)
                .FirstOrDefault(g => g.Id == gameId
                && !g.Players.Any(p => p.Id == player.Id)
                && g.Status == GameStatus.WaitingForPlayers);
            if (game != null)
            {
                game.Players.Add(player.Clone());
                game.PlayersCount = game.Players.Count;
                game.Version++;
            }

            return Task.FromResult(game?.Clone());
        }

        public Task<Game?> AddPlayerToGameAsync(GameType gameType, int? maxPlayers, int? duration, Game.Player player)
        {
            var game = Games.Select(kv => kv.Value).FirstOrDefault(g => g.GameType == gameType
                && (maxPlayers == null || g.MaxPlayers <= maxPlayers)
                && (duration == null || g.MaxDuration <= duration)
                && !g.Players.Any(p => p.Id == player.Id)
                && g.Status == GameStatus.WaitingForPlayers);

            if (game != null)
            {
                game.Players.Add(player.Clone());
                game.PlayersCount = game.Players.Count;
                game.Version++;
            }

            return Task.FromResult(game?.Clone());
        }

        public Task<TGame> CreateGameAsync<TGame>(TGame game) where TGame : Game
        {
            game.Id = Guid.NewGuid().ToString();
            var dbGame = game.Clone();
            Games.TryAdd(game.Id!, dbGame);
            return Task.FromResult(dbGame.Clone());
        }

        public Task<bool> CreatePlayerIfNotExistingAsync(Player player)
        {
            if (Players.ContainsKey(player.Id)) return Task.FromResult(false);
            Players.TryAdd(player.Id, player.Clone());
            return Task.FromResult(true);
        }

        public Task<TGame?> GetAsync<TGame>(string gameId) where TGame : Game
        {
            var game = Games.Select(g => g.Value).OfType<TGame>().FirstOrDefault(g => g.Id == gameId);
            return Task.FromResult(game?.Clone());
        }

        public Task<int> GetNumberOfGamesAsync(string playerId)
        {
            return Task.FromResult(Games.Count(g => g.Value.Players.Any(p => p.Id == playerId)));
        }

        public Task<IEnumerable<Game>> GetPlayableGamesAsync(string playerId, IEnumerable<GameType> gameTypes, IEnumerable<GameStatus> statuses)
        {
            var games = Games.Select(g => g.Value)
                .Where(g => !g.Players.Any(p => p.Id == playerId))
                .Where(g => gameTypes.Contains(g.GameType) && statuses.Contains(g.Status));
            return Task.FromResult(games.Select(g => g.Clone()));
        }

        public Task<IEnumerable<Player.Game>> GetPlayerGamesAsync(string playerId)
        {
            if (!Players.TryGetValue(playerId, out var player))
                return Task.FromResult(Enumerable.Empty<Player.Game>());

            return Task.FromResult(player.Games.Select(g => g.Clone()));
        }

        public Task<IEnumerable<Player.Game>> GetPlayerGamesAsync(string playerId, GameStatus? status)
        {
            if (!Players.TryGetValue(playerId, out var player))
                return Task.FromResult(Enumerable.Empty<Player.Game>());

            return Task.FromResult(player.Games.Where(g => status == null || g.Status == status.Value).Select(g => g.Clone()));
        }

        public Task RemoveAsync(string gameId)
        {
            Games.TryRemove(gameId, out _);
            return Task.CompletedTask;
        }

        public Task<Game?> StartGameAsync(string gameId, IReadOnlyList<int> playerOrders)
        {
            var game = Games.Select(g => g.Value).FirstOrDefault(g => g.Id == gameId && g.Status == GameStatus.WaitingForPlayers);
            if (game != null)
            {
                game.Status = GameStatus.InGame;
                game.Version++;
                for (var i = 0; i < playerOrders.Count; i++)
                {
                    game.Players[i].PlayOrder = playerOrders[i];
                }
            }

            return Task.FromResult(game?.Clone());
        }
    }
}
