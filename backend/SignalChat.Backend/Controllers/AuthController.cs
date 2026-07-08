
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SignalChat.Backend.Database.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace SignalChat.Backend.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _cache;
        
        public class PendingRegistration
        {
            public string Email { get; set; }
            public string Username { get; set; }
            public string Password { get; set; } 
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }

        public AuthController(UserManager<User> userManager,IMemoryCache cache, SignInManager<User> signInManager)
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

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string code)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest("Код подтверждения не указан.");

            
            if (!_cache.TryGetValue(code, out PendingRegistration pending))
                return BadRequest("Неверный или истёкший код подтверждения.");

           
            var user = new User
            {
                UserName = pending.Username,
                Email = pending.Email
            };

            var result = await _userManager.CreateAsync(user, pending.Password);
            if (!result.Succeeded)
                return BadRequest($"Ошибка создания пользователя: {string.Join(", ", result.Errors)}");

            
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            
            _cache.Remove(code);

            return Ok("Email подтверждён, пользователь успешно зарегистрирован!");
        }

        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                // Возвращаем ошибку с понятным сообщением
                return BadRequest(new { error = "Пользователь с таким email уже зарегистрирован." });
            }

            var confirmationCode = Guid.NewGuid().ToString();

            
            var pending = new PendingRegistration
            {
                Email = request.Email,
                Username = request.Username,
                Password = request.Password 
            };
            _cache.Set(confirmationCode, pending, TimeSpan.FromHours(24));

           
            var baseUrl = "https://localhost:7093"; 
            var callbackUrl = $"{baseUrl}/confirm-email?code={WebUtility.UrlEncode(confirmationCode)}";

           
            bool sent = await SendEmailJs(
                "service_ael9hh5",
                "template_7hh3zpm",
                "KQ8zhAP6KrVVGVEOr",
                new
                {
                    to_email = request.Email,
                    message = "Подтвердите вашу почту\n" +
                              $"Пожалуйста, подтвердите регистрацию, перейдя по <a href='{callbackUrl}'>ссылке</a>."
                });

            return Ok(new { message = "Письмо отправлено. Подтвердите email для завершения регистрации." });
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
                
                if (!await _userManager.IsEmailConfirmedAsync(user))
                    return BadRequest("Please confirm your email address before logging in.");
                
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