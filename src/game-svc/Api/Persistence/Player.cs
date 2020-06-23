using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Persistence
{
    public class Player : IEntity
    {
        [BsonId]
        public string ID { get; set; } = null!;
        public DateTime ModifiedOn { get; set; }

        public string Name { get; set; } = null!;

        public int SchemaVersion { get; set; } = 1;

        public List<Game> Games { get; set; } = null!;

        public class Game
        {
            public static Game From(string playerId, Persistence.Game game)
            {
                return new Game()
                {
                    ID = game.ID!,
                    Status = game.Status,
                    GameType = game.GameType,
                    IsOpen = game.IsOpen,
                    AcceptedAt = game.Players.Where(p => p.ID == playerId).Single().AcceptedAt,
                    StartedAt = game.StartedAt,
                    EndedAt = game.EndedAt,
                };
            }

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
