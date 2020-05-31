using IdentityServer4.Models;
using IdentityServer4.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BoardIdentityServer
{
    public class ProfileService : IProfileService
    {
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.IssuedClaims.Add(context.Subject.FindFirst(ClaimTypes.Name));
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.CompletedTask;
        }
    }
}
