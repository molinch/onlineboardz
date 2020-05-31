using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Persistence
{
    public class GameRepository : IGameRepository
    {
        private static readonly FindOneAndUpdateOptions<Game, Game> ReturnDocumentAfterUpdate =
            new FindOneAndUpdateOptions<Game, Game>
            {
                ReturnDocument = ReturnDocument.After
            };

        private readonly IMongoCollection<Game> _waitingRooms;

        public GameRepository(DatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.GameDatabaseName);
            _waitingRooms = database.GetCollection<Game>(nameof(Game));
        }

        public async Task<IEnumerable<Game>> GetAsync()
        {
            var find = await _waitingRooms.FindAsync(r => true);
            return await find.ToListAsync();
        }

        public async Task<Game> GetAsync(string id)
        {
            var find = await _waitingRooms.FindAsync(r => r.Id == id);
            return await find.FirstOrDefaultAsync();
        }

        public async Task<bool> IsAlreadyInWaitingRoomAsync(string playerId)
        {
            var find = await _waitingRooms.FindAsync(r => r.Metadata.Players.Any(p => p.Id == playerId));
            return await find.AnyAsync();
        }

        public async Task<Game> CreateAsync(Game room)
        {
            await _waitingRooms.InsertOneAsync(room);
            return room;
        }

        public Task<Game> AddPlayerIfNotThereAsync(string id, Game.GameMetadata.Player player) =>
            _waitingRooms.FindOneAndUpdateAsync<Game>(
                r => r.Id == id && !r.Metadata.Players.Any(p => p.Id == player.Id), // && r.Metadata.MaxPlayers > r.Metadata.Players.Count
                Builders<Game>.Update.AddToSet(r => r.Metadata.Players, player),
                ReturnDocumentAfterUpdate);

        public Task RemoveAsync(Game room) =>
            _waitingRooms.DeleteOneAsync(r => r.Id == room.Id);

        public Task RemoveAsync(string id) =>
            _waitingRooms.DeleteOneAsync(room => room.Id == id);
    }
}
