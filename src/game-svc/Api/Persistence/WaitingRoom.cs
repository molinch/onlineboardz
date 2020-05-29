using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Api.Persistence
{
    public class WaitingRoom
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string GameId { get; set; } = null!;

        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }

        public bool IsOpen { get; set; }

        public List<Player> Players { get; set; } = null!;
    }
}
