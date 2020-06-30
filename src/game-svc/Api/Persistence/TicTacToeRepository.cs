using Api.Domain;
using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Threading.Tasks;

namespace Api.Persistence
{
    public class TicTacToeRepository : ITicTacToeRepository
    {
        private readonly DB _database;

        public TicTacToeRepository(DB database) : base()
        {
            _database = database;
        }

        public async Task SetTicTacToeStepAsync(TicTacToe game, int cellIndex)
        {
            TicTacToe.CellData cell = game!.Cells[cellIndex]!;

            var update = _database.Update<Game>()
                .Match(g => g.ID == game.ID
                        && g.GameType == game.GameType
                        && g.Status == GameStatus.InGame
                        && g.Version == game.Version)
                .Match($"{{ 'Cells.{cellIndex}': {{ $type: 10 }} }}") // $type 10 is null
                .Modify($"{{ $set : {{ 'Cells.{cellIndex}' : {{ 'Number': {game.TickedCellsCount} }} }} }}")
                .Modify(g => g.Inc(g => g.Version, 1));

            if (game.Status == GameStatus.Finished)
            {
                var bsonPlayers = game.ToBsonDocument().GetElement("Players").Value.ToString();
                update = update
                    .Modify($"{{ $set : {{ 'Status' : {(int)game.Status} }} }}")
                    .Modify($"{{ $set : {{ 'EndedAt' : '{DateTime.Now.ToIso()}' }} }}")
                    .Modify($"{{ $set : {{ 'Players' : {bsonPlayers} }} }}");
            }

            var updateResult = await update
                 .ExecuteAsync();

            if (updateResult.MatchedCount == 0 || updateResult.ModifiedCount == 0) throw new UpdateException();
        }
    }
}
