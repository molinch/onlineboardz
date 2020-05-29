using System;

namespace BoardIdentityServer.Persistence
{
    public class User
    {
        public int Id { get; set; }
        public string ExternalId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PictureUrl { get; set; } = null!;
        public string Locale { get; set; } = null!;
        public string Provider { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
