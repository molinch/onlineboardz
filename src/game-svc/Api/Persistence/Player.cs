using System;

namespace Api.Persistence
{
    public class Player
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTime AcceptedAt { get; set; }
    }
}
