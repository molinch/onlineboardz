using Api.Domain;
using Api.Persistence;
using System.Collections.Generic;
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
        private Dictionary<string, Player> Players => _db.Players;
        private Dictionary<string, Game> Games => _db.Games;

        public Task SetTicTacToeStepAsync(TicTacToe game, int cellIndex)
        {
            if (!Games.TryGetValue(game.Id!, out var dbGame))
            {
                Games.Remove(game.Id!);
                Games.Add(game.Id!, game);
            }

            return Task.CompletedTask;
        }
    }
}
