using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using SignalChat.Backend.Database.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace SignalChat.Backend.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IOptions<OpenIddictServerOptions> _options;

        public AuthController(UserManager<User> userManager, IOptions<OpenIddictServerOptions> options, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _options = options;
            _signInManager = signInManager;
        }

        public class RegisterRequest
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = new User
            {
                UserName = request.Username,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "User registered successfully", userId = user.Id });
        }

        [HttpPost("/connect/token")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            
            if (request.IsPasswordGrantType())
            {
                var user = await _userManager.FindByEmailAsync(request.Username!);

                if (user == null)
                {
                    return Forbid();
                }

                var result = await _signInManager.CheckPasswordSignInAsync(
                    user,
                    request.Password!,
                    false);

                if (!result.Succeeded)
                {
                    return Forbid();
                }

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

                return SignIn(
                    principal,
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            if (request.IsRefreshTokenGrantType())
            {
                var result = await HttpContext.AuthenticateAsync(
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                var userId = result.Principal!.GetClaim(
                    OpenIddictConstants.Claims.Subject);

                var user = await _userManager.FindByIdAsync(userId!);

                if (user == null)
                {
                    return Forbid();
                }

                var identity = new ClaimsIdentity(
                    result.Principal.Claims,
                    TokenValidationParameters.DefaultAuthenticationType);

                var principal = new ClaimsPrincipal(identity);

                return SignIn(
                    principal,
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new NotImplementedException();
        }
    }
}