using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Persistence
{
    public class GameRepository : IGameRepository
    {
        private readonly DB _database;

        public GameRepository(DB database)
        {
            _database = database;

            _database.Index<Game>()
              .Key(g => g.Status, KeyType.Ascending)
              .CreateAsync();
        }

        public async Task<IEnumerable<Game>> GetAsync()
        {
            return await _database.Queryable<Game>().ToListAsync();
        }

        public Task<Game> GetAsync(string id)
        {
            return _database.Find<Game>().OneAsync(id);
        }

        public Task<int> GetNumberOfGamesAsync(string playerId)
        {
            return _database.Queryable<Game>()
                .Where(g => g.Metadata.Players.Any(p => p.ID == playerId))
                .Where(g => g.Status != GameStatus.Finished && g.Status != GameStatus.TimedOut)
                .CountAsync();
        }

        public async Task<Game> CreateAsync(Game game)
        {
            await _database.SaveAsync(game);
            return game;
        }

        public Task<Game> AddPlayerIfNotThereAsync(string id, Game.GameMetadata.Player player)
        {
            return _database.UpdateAndGet<Game>()
                .Match(g => g.ID == id && !g.Metadata.Players.Any(p => p.ID == player.ID))
                .Modify(g => g.Push(g => g.Metadata.Players, player))
                .Modify(g => g.Inc(g => g.Metadata.PlayersCount, 1))
                .ExecuteAsync();
        }

        public Task<Game> SetGameStatusAsync(string id, GameStatus oldStatus, GameStatus newStatus) =>
            _database.UpdateAndGet<Game>()
                .Match(g => g.ID == id && g.Status == oldStatus)
                .Modify(g => g.Status, newStatus)
                .ExecuteAsync();

        public async Task RemoveAsync(string id) =>
            await _database.DeleteAsync<Game>(id);
    }
}
