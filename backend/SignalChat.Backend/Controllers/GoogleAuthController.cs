using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SignalChat.Backend.Database.Entities;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace SignalChat.Backend.Controllers
{

    [ApiController]
    [Route("api/auth/google")]
    public class GoogleAuthController : ControllerBase
    {
        [HttpGet("login")]
        public IActionResult Login()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/api/auth/google/callback"
            };
            return Challenge(
            properties,
            GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromServices] UserManager<User> userManager)
        {
            var result = await HttpContext.AuthenticateAsync(
                IdentityConstants.ExternalScheme);
            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    result.None,
                    result.Succeeded,
                    Failure = result.Failure?.Message
                });
            }
            var email = result.Principal?
             .FindFirst(ClaimTypes.Email)?
             .Value;
            if (email == null)
            {
                return BadRequest();
            }
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user);
            }
            var identity = new ClaimsIdentity(
                IdentityConstants.ApplicationScheme);
            identity.SetClaim(
            OpenIddictConstants.Claims.Subject,
            user.Id);
            identity.SetClaim(
            OpenIddictConstants.Claims.Email,
            user.Email!);
            identity.SetScopes(new[]
             {

                OpenIddictConstants.Scopes.OfflineAccess
               });
            var principal = new ClaimsPrincipal(identity);
            return SignIn(
            principal,
            IdentityConstants.ApplicationScheme);
        }

    }
}
