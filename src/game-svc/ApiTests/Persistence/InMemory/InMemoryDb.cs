using Api.Domain;
using System.Collections.Concurrent;

namespace ApiTests.Persistence
{
    public class InMemoryDb
    {
        // These aren't thread safe but that's fine we are in tests
        public ConcurrentDictionary<string, Player> Players { get; } = new ConcurrentDictionary<string, Player>();
        public ConcurrentDictionary<string, Game> Games { get; } = new ConcurrentDictionary<string, Game>();
    }
}
