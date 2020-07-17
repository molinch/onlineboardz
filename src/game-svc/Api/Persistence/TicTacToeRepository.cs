using Api.Domain;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Thin;
using System;
using System.Threading.Tasks;

namespace Api.Persistence
{
    public class TicTacToeRepository : ITicTacToeRepository
    {
        private readonly IMongoCollection<Game> _games;

        public TicTacToeRepository(IMongoDatabase database) : base()
        {
            _games = database.Collection<Game>();
        }

        public async Task SetTicTacToeStepAsync(TicTacToe game, int cellIndex)
        {
            TicTacToe.CellData cell = game!.Cells[cellIndex]!;

            var command = _games.UpdateOne()
                .Filter(b => b
                    .Match(g => g.Id == game.Id
                        && g.GameType == game.GameType
                        && g.Status == GameStatus.InGame
                        && g.Version == game.Version)
                    .Match($"{{ 'Cells.{cellIndex}': {{ $type: 10 }} }}")) // $type 10 is null
                .Update(b => b
                    .Modify($"{{ $set : {{ 'Cells.{cellIndex}' : {{ 'Number': {game.TickedCellsCount} }} }} }}")
                    .Modify(g => g.Inc(g => g.Version, 1)));

            if (game.Status == GameStatus.Finished)
            {
                var bsonPlayers = game.ToBsonDocument().GetElement("Players").Value.ToString();
                command = command
                    .Update(b => b
                        .Modify($"{{ $set : {{ 'Status' : {(int)game.Status} }} }}")
                        .Modify($"{{ $set : {{ 'EndedAt' : '{DateTime.Now.ToIso()}' }} }}")
                        .Modify($"{{ $set : {{ 'Players' : {bsonPlayers} }} }}"));
            }

            var updateResult = await command.ExecuteAsync();

            if (updateResult.MatchedCount == 0 || updateResult.ModifiedCount == 0) throw new UpdateException();
        }
    }
}
