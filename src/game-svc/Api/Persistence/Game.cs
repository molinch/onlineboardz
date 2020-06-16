using MongoDB.Entities.Core;
using System;
using System.Collections.Generic;

namespace Api.Persistence
{
    public class Game: Entity
    {
        public GameStatus Status { get; set; }

        public object? State { get; set; }

        public GameMetadata Metadata { get; set; } = null!;

        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public class GameMetadata
        {
            public int SchemaVersion { get; set; }

            public GameType GameType { get; set; }

            /// <summary>
            /// If greater than 0, specified the maximum time in minutes this game may take.
            /// The game status will change to TimedOut once that duration has been hit.
            /// </summary>
            public int MaxDuration { get; set; }

            // if we have a timeout then we could have a number of players between min/max, good idea? overkill?
            // or initiator can always force to start the game once it's between min/max players
            public int MinPlayers { get; set; }
            public int MaxPlayers { get; set; }
            public int PlayersCount { get; set; }

            public bool IsOpen { get; set; }

            public List<Player> Players { get; set; } = null!;

            public class Player
            {
                public string ID { get; set; } = null!;
                public string Name { get; set; } = null!;
                public DateTime AcceptedAt { get; set; }
            }
        }
    }
}
