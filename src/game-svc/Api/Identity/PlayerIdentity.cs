using IdentityModel;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Api.Extensions
{
    public class PlayerIdentity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PlayerIdentity(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string PlayerId =>
            _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == JwtClaimTypes.Subject).Value;

        public string Name =>
            _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == JwtClaimTypes.Name).Value;
    }
}
