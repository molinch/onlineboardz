using Api.Domain;
using System.Collections.Generic;

namespace ApiTests.Persistence
{
    public class InMemoryDb
    {
        // These aren't thread safe but that's fine we are in tests
        public Dictionary<string, Player> Players { get; } = new Dictionary<string, Player>();
        public Dictionary<string, Game> Games { get; } = new Dictionary<string, Game>();
    }
}
