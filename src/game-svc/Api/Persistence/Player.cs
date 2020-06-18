using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities.Core;
using System;
using System.Collections.Generic;

namespace Api.Persistence
{
    public class Player : Entity
    {
        public string Name { get; set; } = null!;

        public int SchemaVersion { get; set; } = 1;

        public List<Game> Games { get; set; } = null!;

        public class Game
        {
            [BsonId]
            public string ID { get; set; } = null!;
            public GameStatus Status { get; set; }
            public GameType GameType { get; set; }
            public bool IsOpen { get; set; }
            public DateTime AcceptedAt { get; set; }
            public DateTime? StartedAt { get; set; }
            public DateTime? EndedAt { get; set; }
        }
    }
}
