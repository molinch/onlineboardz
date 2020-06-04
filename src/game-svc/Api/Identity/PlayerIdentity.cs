using IdentityModel;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;

namespace Api.Extensions
{
    public class PlayerIdentity
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PlayerIdentity(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Id =>
            _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

        public string Name =>
            _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == JwtClaimTypes.Name).Value;

        public string Email =>
            _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Email).Value;

        public string PictureUrl =>
            _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == JwtClaimTypes.Picture).Value;
    }
}
