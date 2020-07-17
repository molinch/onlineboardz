using Api.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.TransferObjects
{
    public class Player
    {
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;

        public List<Game> Games { get; set; } = null!;

        public class Game
        {
            public static Game From(string playerId, Domain.Game game)
            {
                return new Game()
                {
                    ID = game.Id!,
                    Status = game.Status,
                    GameType = game.GameType,
                    IsOpen = game.IsOpen,
                    AcceptedAt = game.Players.Where(p => p.Id == playerId).Single().AcceptedAt,
                    StartedAt = game.StartedAt,
                    EndedAt = game.EndedAt,
                };
            }

            public string ID { get; set; } = null!;
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
