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

        public async Task<TicTacToe> SetTicTacToeStepAsync(string? nextPlayerId, string gameId, int cellIndex, bool value, int stepNumber, GameStatus nextStatus)
        {
            var game = (TicTacToe)await _database.UpdateAndGet<Game>()
                 .Match(g => g.ID == gameId && g.Status == GameStatus.InGame && ((TicTacToe)g).Cells[cellIndex] == null)
                 .Modify($"{{ $set : {{ 'Status' : {(int)nextStatus} }} }}")
                 .Modify($"{{ $set : {{ 'NextPlayerId' : '{nextPlayerId}' }} }}")
                 .Modify($"{{ $set : {{ 'Cells.{cellIndex}' : {{ 'Step': {value.ToString().ToLowerInvariant()}, 'Number': {stepNumber} }} }} }}")
                 .Option(g => g.ReturnDocument = MongoDB.Driver.ReturnDocument.After)
                 .ExecuteAsync();

            if (game == null) throw new UpdateException();

            return game;
        }
    }
}
