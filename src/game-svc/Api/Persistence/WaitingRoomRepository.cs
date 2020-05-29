using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Persistence
{
    public class WaitingRoomRepository : IWaitingRoomRepository
    {
        private readonly IMongoCollection<WaitingRoom> _waitingRooms;

        public WaitingRoomRepository(DatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.GameDatabaseName);
            _waitingRooms = database.GetCollection<WaitingRoom>(nameof(WaitingRoom));
        }

        public async Task<IEnumerable<WaitingRoom>> GetAsync()
        {
            var find = await _waitingRooms.FindAsync(r => true);
            return await find.ToListAsync();
        }

        public async Task<WaitingRoom> GetAsync(string id)
        {
            var find = await _waitingRooms.FindAsync(r => r.Id == id);
            return await find.FirstOrDefaultAsync();
        }

        public async Task<WaitingRoom> CreateAsync(WaitingRoom room)
        {
            await _waitingRooms.InsertOneAsync(room);
            return room;
        }

        public Task UpdateAsync(string id, WaitingRoom room) =>
            _waitingRooms.ReplaceOneAsync(r => r.Id == id, room);

        public Task RemoveAsync(WaitingRoom room) =>
            _waitingRooms.DeleteOneAsync(r => r.Id == room.Id);

        public Task RemoveAsync(string id) =>
            _waitingRooms.DeleteOneAsync(room => room.Id == id);
    }
}
