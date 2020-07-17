using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Domain
{
    public class Player
    {
        [BsonId]
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;

        public int SchemaVersion { get; set; } = 1;

        public List<Game> Games { get; set; } = null!;

        public class Game
        {
            public static Game From(string playerId, Domain.Game game)
            {
                return new Game()
                {
                    Id = game.Id!,
                    Status = game.Status,
                    GameType = game.GameType,
                    IsOpen = game.IsOpen,
                    AcceptedAt = game.Players.Where(p => p.Id == playerId).Single().AcceptedAt,
                    StartedAt = game.StartedAt,
                    EndedAt = game.EndedAt,
                };
            }

            [BsonId]
            public string Id { get; set; } = null!;
            public GameStatus Status { get; set; }
            public GameType GameType { get; set; }
            public bool IsOpen { get; set; }
            public DateTime AcceptedAt { get; set; }
            public DateTime? StartedAt { get; set; }
            public DateTime? EndedAt { get; set; }
            public PlayerGameStatus PlayerGameStatus { get; set; }
        }
    }
}
