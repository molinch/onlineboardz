using Api.Domain;
using Api.Persistence;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiTests.Persistence
{
    public class InMemoryTicTacToeRepository : ITicTacToeRepository
    {
        private readonly InMemoryDb _db;

        public InMemoryTicTacToeRepository(InMemoryDb db)
        {
            _db = db;
        }
        private ConcurrentDictionary<string, Player> Players => _db.Players;
        private ConcurrentDictionary<string, Game> Games => _db.Games;

        public Task SetTicTacToeStepAsync(TicTacToe game, int cellIndex)
        {
            var dbGame = Games.Select(g => g.Value).OfType<TicTacToe>().FirstOrDefault(g => g.Id == game.Id
                && game.Cells[cellIndex] == null
                && g.Status == GameStatus.InGame);

            if (dbGame != null)
            {
                Games.Remove(game.Id!, out _);
                dbGame = game.Clone();
                dbGame.Cells[cellIndex] = new TicTacToe.CellData() { Number = dbGame.TickedCellsCount };
                dbGame.Version++;
                if (dbGame.Status == GameStatus.Finished)
                {
                    dbGame.EndedAt = DateTime.UtcNow;
                }
                Games.TryAdd(dbGame.Id!, dbGame);
            }

            if (dbGame == null) throw new UpdateException();

            return Task.CompletedTask;
        }
    }
}
