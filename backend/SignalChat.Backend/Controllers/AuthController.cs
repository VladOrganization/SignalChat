using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using SignalChat.Backend.Database.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SignalChat.Backend.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        
        private readonly IDistributedCache _cache;

        public AuthController(UserManager<User> userManager, IDistributedCache cache, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _cache = cache;
            _signInManager = signInManager;
        }

        public class RegisterRequest
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public record AproveCodeRequest(string id,string code);
        [HttpPost("aprove-code")]
        public async Task<IActionResult> AproveCode([FromBody] AproveCodeRequest request) {
            var code = await _cache.GetStringAsync(request.id);

            if (code == request.code)
            {
                return Ok();
            }

            return BadRequest("code is not aprove");
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
            Random rnd = new Random();
            var emailCode = rnd.Next(1111,9999);
            bool sent = await SendEmailJs("service_ael9hh5", "template_7hh3zpm", "KQ8zhAP6KrVVGVEOr", 
                new { to_email = request.Email, message = emailCode });
            await _cache.SetStringAsync(user.Id.ToString(), emailCode.ToString());
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


        public static async Task<bool> SendEmailJs(string serviceId, string templateId, string userId, object templateParams, string accessToken = "LMdWYRopidtHIyiNq6LMg")
        {
            using var client = new HttpClient();
            var body = new { service_id = serviceId, template_id = templateId, user_id = userId, template_params = templateParams, accessToken };
            var response = await client.PostAsync("https://api.emailjs.com/api/v1.0/email/send",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            return response.IsSuccessStatusCode;
        }
    }
}