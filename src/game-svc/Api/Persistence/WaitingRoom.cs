using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Api.Persistence
{
    public class WaitingRoom
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public GameType GameType { get; set; }
        public TimeSpan MaxDuration { get; set; }

        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }

        public bool IsOpen { get; set; }

        public List<Player> Players { get; set; } = null!;

        public class Player
        {
            public string Id { get; set; } = null!;
            public string Name { get; set; } = null!;
            public DateTime AcceptedAt { get; set; }
        }
    }
}
