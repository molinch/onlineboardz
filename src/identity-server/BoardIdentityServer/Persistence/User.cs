using System;

namespace BoardIdentityServer.Persistence
{
    public class User
    {
        /// <summary>
        /// Internal id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Id used outside of this service
        /// We don't want a sequential number as discovering data is way easier
        /// </summary>
        public Guid ExternalId { get; set; }

        /// <summary>
        /// Id received from the provider (Google, Facebook)
        /// </summary>
        public string ProviderId { get; set; } = null!;

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
