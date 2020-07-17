namespace ApiTests.Persistence
{
    // Will guarantee that our InMemory repositories, that are used for testing, behave as the regular ones
    public class InMemoryTicTacToeRepositoryTests : TicTacToeRepositoryTests
    {
        public InMemoryTicTacToeRepositoryTests() : base()
        {
            var inMemoryDb = new InMemoryDb();
            _gameRepository = new InMemoryGameRepository(inMemoryDb);
            _ticTacToeRepository = new InMemoryTicTacToeRepository(inMemoryDb);
        }
    }
}
