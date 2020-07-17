namespace ApiTests.Persistence
{
    // Will guarantee that our InMemory repositories, that are used for testing, behave as the regular ones
    public class InMemoryGameRepositoryTests : GameRepositoryTests
    {
        public InMemoryGameRepositoryTests() : base()
        {
            _repository = new InMemoryGameRepository(new InMemoryDb());
        }
    }
}
