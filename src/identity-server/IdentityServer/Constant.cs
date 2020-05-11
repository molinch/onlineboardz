using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer
{
    public static class ExtraClaims
    {
        private static readonly HashSet<string> Claims = new HashSet<string> { "name", "picture", "locale" };

        public static bool IsExtra(Claim claim) => Claims.Contains(claim.Type);
    }
}
