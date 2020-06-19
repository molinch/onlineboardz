using MongoDB.Entities;
using System.Threading.Tasks;

namespace Api.Persistence
{
    public class TicTacToeRepository : ITicTacToeRepository
    {
        private readonly DB _database;

        public TicTacToeRepository(DB database)
        {
            _database = database;
        }

        public async Task<TicTacToeGame> SetTicTacToeStepAsync(string gameId, int stepIndex, bool value)
        {
            var game = await _database.UpdateAndGet<TicTacToeGame>()
                 .Match(g => g.ID == gameId)
                 .Modify($"{{ $set : {{ 'Steps.{stepIndex}' : {value.ToString().ToLowerInvariant()} }} }}")
                 .ExecuteAsync();

            if (game == null) throw new UpdateException();

            return game;
        }
    }
}
