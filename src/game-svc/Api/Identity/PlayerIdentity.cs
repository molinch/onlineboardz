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
            _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Name).Value;
    }
}
