using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SignalChat.Backend.Database.Entities;
using System.Security.Claims;

namespace SignalChat.Backend.Controllers
{
    [ApiController]
    [Route("api/auth/google")]
    public class GoogleAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;

        public GoogleAuthController(IConfiguration configuration, IMemoryCache cache)
        {
            _configuration = configuration;
            _cache = cache;
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

            // Выдаём одноразовый код обмена вместо токена напрямую: сам access/refresh
            // токен должен выпускаться через OpenIddict (/connect/token), иначе
            // у Google-пользователей никогда не будет refresh-токена.
            var code = Guid.NewGuid().ToString();
            _cache.Set(code, user.Id, TimeSpan.FromMinutes(1));

            string front_url = _configuration["Front-Url:Url"];
            return Redirect($"{front_url}/auth/callback?code={Uri.EscapeDataString(code)}");
        }
    }
}