using IdentityModel;
using BoardIdentityServer.Persistence;
using IdentityServer4.Events;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4;

namespace BoardIdentityServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticateController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IWebHostEnvironment _environment;
        private readonly IEventService _events;
        private readonly ILogger<AuthenticateController> _logger;
        private readonly UserDbContext _usersDbContext;

        public AuthenticateController(
            IIdentityServerInteractionService interaction,
            IWebHostEnvironment environment,
            IEventService events,
            ILogger<AuthenticateController> logger,
            UserDbContext users)
        {
            _interaction = interaction;
            _environment = environment;
            _events = events;
            _logger = logger;
            _usersDbContext = users;
        }

        [HttpGet]
        [Route("ExternalLogin")]
        public IActionResult ExternalLogin([Required] string provider, [Required]string returnUrl)
        {
            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                return BadRequest("Invalid ReturnUrl");
            }

            // start challenge and roundtrip the return URL and scheme 
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(HandleCallback)).ToLower(),
                Items = {
                    { "returnUrl", returnUrl },
                    { "scheme", provider }
                }
            };

            return Challenge(props, provider);
        }

        [HttpGet]
        [Route("HandleCallback")]
        public async Task<IActionResult> HandleCallback()
        {
            // read external identity from the temporary cookie
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result == null)
            {
                // should probably do something with the error, redirect back to SPA
                throw new Exception("Result is null");
            }

            if (result.Succeeded != true)
            {
                // should probably do something with the error, redirect back to SPA
                throw new Exception("External authentication error");
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
                _logger.LogDebug("External claims: {@claims}", externalClaims);
            }

            // retrieve return URL
            var returnUrl = result.Properties?.Items["returnUrl"];
            if (returnUrl == null)
            {
                // should probably do something with the error, redirect back to SPA
                throw new Exception("Return url is empty");
            }

            // lookup our user and external provider info
            var (user, provider, claims) = FindUserFromExternalProvider(result);
            var externalUser = result.Principal;
            if (user == null)
            {
                // this might be where you might initiate a custom workflow for user registration
                // in this sample we don't show how that would be done, as our sample implementation
                // simply auto-provisions new external user
                user = new User()
                {
                    ExternalId = Guid.NewGuid(),
                    ProviderId = externalUser.FindFirst(ClaimTypes.NameIdentifier).Value,
                    Name = externalUser.FindFirst(ClaimTypes.Name).Value,
                    FirstName = externalUser.FindFirst(ClaimTypes.GivenName).Value,
                    LastName = externalUser.FindFirst(ClaimTypes.Surname).Value,
                    Email = externalUser.FindFirst(ClaimTypes.Email).Value,
                    PictureUrl = GetImageUrl(provider, externalUser),
                    Locale = externalUser.FindFirst(JwtClaimTypes.Locale)?.Value ?? "en",
                    Provider = provider,
                    CreatedAt = DateTime.UtcNow
                };

                _usersDbContext.Add(user);
                await _usersDbContext.SaveChangesAsync();
            }

            // this allows us to collect any additional claims or properties
            // for the specific protocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            var additionalLocalClaims = new List<Claim>()
            {
                new Claim(JwtClaimTypes.Name, user.Name),
                new Claim(JwtClaimTypes.Email, user.Email),
                new Claim(JwtClaimTypes.Picture, user.PictureUrl)
            };
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);

            // issue authentication cookie for user
            var userExternalId = user.ExternalId.ToString();
            var identityServerUser = new IdentityServerUser(userExternalId)
            {
                DisplayName = user.Name,
                IdentityProvider = provider,
                AdditionalClaims = additionalLocalClaims
            };

            await HttpContext.SignInAsync(identityServerUser, localSignInProps);

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // check if external login is in the context of an OIDC request
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, userExternalId, user.Name, user.Name, true, context?.ClientId));

            return Redirect(returnUrl);
        }

        private string GetImageUrl(string provider, ClaimsPrincipal principal)
        {
            switch (provider)
            {
                case Provider.Google:
                    return principal.FindFirst(JwtClaimTypes.Picture).Value;
                case Provider.Facebook:
                    var userId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
                    return $"https://graph.facebook.com/{userId}/picture?type=normal"; // square, small, normal or large
                default:
                    return "";
            }
        }

        private void ProcessLoginCallbackForOidc(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var id_token = externalResult.Properties.GetTokenValue("id_token");
            if (id_token != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });
            }
        }

        private (User user, string provider, IReadOnlyList<Claim> claims) FindUserFromExternalProvider(AuthenticateResult result)
        {
            var externalUser = result.Principal;
            var email = externalUser.FindFirst(ClaimTypes.Email).Value;
            var user = _usersDbContext.Users.FirstOrDefault(u => u.Email == email);

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims
                .Where(c => c.Type != JwtClaimTypes.Subject && c.Type != ClaimTypes.NameIdentifier)
                .ToList();

            var provider = result.Properties.Items["scheme"];
            return (user, provider, claims);
        }

        [HttpGet]
        [Route("Logout")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            var context = await _interaction.GetLogoutContextAsync(logoutId);
            bool showSignoutPrompt = true;

            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                showSignoutPrompt = false;
            }

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await HttpContext.SignOutAsync();
            }

            // no external signout supported for now (see \Quickstart\Account\AccountController.cs TriggerExternalSignout)
            return Ok(new
            {
                showSignoutPrompt,
                ClientName = string.IsNullOrEmpty(context?.ClientName) ? context?.ClientId : context?.ClientName,
                context?.PostLogoutRedirectUri,
                context?.SignOutIFrameUrl,
                logoutId
            });
        }

        [HttpGet]
        [Route("Error")]
        public async Task<IActionResult> Error(string errorId)
        {
            // retrieve error details from identityserver
            var message = await _interaction.GetErrorContextAsync(errorId);

            if (message != null)
            {
                if (!_environment.IsDevelopment())
                {
                    // only show in development
                    message.ErrorDescription = null;
                }
            }

            return Ok(message);
        }
    }
}