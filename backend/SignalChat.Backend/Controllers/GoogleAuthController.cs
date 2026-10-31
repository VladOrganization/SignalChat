using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using SignalChat.Backend.Database.Entities;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace SignalChat.Backend.Controllers
{
    [ApiController]
    [Route("api/auth/google")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly IOptions<OpenIddictServerOptions> _oidcOptions;

        public GoogleAuthController(IOptions<OpenIddictServerOptions> oidcOptions)
        {
            _oidcOptions = oidcOptions;
        }

        [HttpGet("login")]
        public IActionResult Login(string? redirect_uri)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/api/auth/google/callback"
            };
            if (!string.IsNullOrEmpty(redirect_uri))
            {
                properties.Items["frontend_redirect_uri"] = redirect_uri;
            }

            return Challenge(
                properties,
                GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(
            [FromServices] UserManager<User> userManager)
        {
            var result = await HttpContext.AuthenticateAsync(
                IdentityConstants.ExternalScheme);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    succeeded = false,
                    failure = result.Failure?.Message
                });
            }

            var email = result.Principal?
                .FindFirst(ClaimTypes.Email)?
                .Value;

            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new
                {
                    succeeded = false,
                    error = "Google account email was not found."
                });
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

                var createResult = await userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    return BadRequest(new
                    {
                        succeeded = false,
                        error = "Failed to create user.",
                        errors = createResult.Errors.Select(x => x.Description)
                    });
                }
            }

            var options = _oidcOptions.Value;

            var signingCredentials = options.SigningCredentials.FirstOrDefault();

            if (signingCredentials == null)
            {
                return StatusCode(500, new
                {
                    error = "Signing credentials not configured in OpenIddict."
                });
            }

            // Создаём claims
            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType);

            identity.SetClaim(
                OpenIddictConstants.Claims.Subject,
                user.Id.ToString());

            identity.SetClaim(
                OpenIddictConstants.Claims.Email,
                user.Email!);

            identity.SetScopes(
                OpenIddictConstants.Scopes.OfflineAccess);

            var claims = identity.Claims.ToList();

            if (!claims.Any(x => x.Type == OpenIddictConstants.Claims.Scope))
            {
                claims.Add(
                    new Claim(
                        OpenIddictConstants.Claims.Scope,
                        OpenIddictConstants.Scopes.OfflineAccess));
            }

            // JWT
            var tokenHandler = new JwtSecurityTokenHandler();

            var expires = DateTime.UtcNow.Add(
                options.AccessTokenLifetime ??
                TimeSpan.FromMinutes(15));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),

                Expires = expires,

                Issuer = options.Issuer?.ToString()
                         ?? "https://localhost:7093/",

                SigningCredentials = signingCredentials,

                TokenType = "at+jwt"
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);

            var accessToken = tokenHandler.WriteToken(securityToken);

            // Возвращаем JSON
            return Redirect($"http://localhost:5173/auth/callback?token={Uri.EscapeDataString(accessToken)}");
        }
    }
}