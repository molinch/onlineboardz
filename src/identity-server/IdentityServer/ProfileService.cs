using IdentityServer4.Models;
using IdentityServer4.Services;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer
{
    public class ProfileService : IProfileService
    {
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.IssuedClaims.AddRange(context.Subject.Claims.Where(ExtraClaims.IsExtra));
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context) =>Task.CompletedTask;
    }
}
