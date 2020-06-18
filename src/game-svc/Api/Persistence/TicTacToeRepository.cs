using MongoDB.Entities;
using System.Threading.Tasks;

namespace Api.Persistence
{
    public class TicTacToeRepository
    {
        private readonly DB _database;

        public TicTacToeRepository(DB database)
        {
            _database = database;
        }

        public async Task<TicTacToeGame?> SetTicTacToeStepAsync(string id, int index, bool value) =>
            await _database.UpdateAndGet<TicTacToeGame>()
                 .Match(g => g.ID == id)
                 .Modify($"{{ $set : {{ 'Games.$[{index}].Status' : {value} }} }}")
                 .ExecuteAsync();
    }
}
