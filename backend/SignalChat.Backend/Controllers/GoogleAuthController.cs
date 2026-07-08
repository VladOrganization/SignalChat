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

            var options = _oidcOptions.Value;
            var signingCredentials = options.SigningCredentials.FirstOrDefault();
            if (signingCredentials == null)
            {
                return StatusCode(500, "Signing credentials not configured in OpenIddict.");
            }

            // Build the principal exactly like in AuthController
            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType);

            identity.SetClaim(OpenIddictConstants.Claims.Subject, user.Id);
            identity.SetClaim(OpenIddictConstants.Claims.Email, user.Email!);

            identity.SetScopes(new[]
            {
                OpenIddictConstants.Scopes.OfflineAccess
            });

            var principal = new ClaimsPrincipal(identity);

            principal.SetScopes(new[]
            {
                OpenIddictConstants.Scopes.OfflineAccess
            });

            // Sign the JWT token manually using OpenIddict signing credentials
            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = principal.Claims.ToList();
            
            // Add the scope claim to JWT so it validates
            if (!claims.Any(c => c.Type == "scope"))
            {
                claims.Add(new Claim("scope", OpenIddictConstants.Scopes.OfflineAccess));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme),
                Expires = DateTime.UtcNow.Add(options.AccessTokenLifetime ?? TimeSpan.FromMinutes(15)),
                Issuer = options.Issuer?.ToString() ?? "https://localhost:7093/",
                SigningCredentials = signingCredentials,
                TokenType = "at+jwt" // CRITICAL: Tells OpenIddict that this is a valid access token
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            // Read frontend_redirect_uri from authentication properties
            string frontendRedirectUri = "http://localhost:5173/";
            if (result.Properties?.Items.TryGetValue("frontend_redirect_uri", out var savedUri) == true && !string.IsNullOrEmpty(savedUri))
            {
                frontendRedirectUri = savedUri;
            }

            var redirectUrl = $"{frontendRedirectUri.TrimEnd('/')}/?access_token={accessToken}&token_type=Bearer&expires_in={(int)(options.AccessTokenLifetime ?? TimeSpan.FromMinutes(15)).TotalSeconds}";
            return Redirect(redirectUrl);
        }
    }
}
