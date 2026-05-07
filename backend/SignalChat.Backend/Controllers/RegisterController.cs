using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using SignalChat.Backend.Database.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SignalChat.Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IServiceProvider _serviceProvider;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _serviceProvider = serviceProvider;
        }

        public class RegisterRequest
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("register-oid")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request.");

            var user = new User
            {
                UserName = request.Username,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { errors });
            }

            

            return Ok(new { message = "User registered successfully", userId = user.Id });
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginJson([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UsernameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Username/email and password are required." });

            // Поиск пользователя (логин или email)
            var user = await _userManager.FindByNameAsync(request.UsernameOrEmail);
            if (user == null && request.UsernameOrEmail.Contains('@'))
                user = await _userManager.FindByEmailAsync(request.UsernameOrEmail);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return Unauthorized(new { error = "Invalid credentials." });

            // Генерация JWT access token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = GetSigningKey(); // Получаем ключ подписи из конфигурации OpenIddict

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = "https://localhost:5001", // Ваш issuer
                Audience = "SignalChat.Api",        // Audience вашего API
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
            };

            var accessToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessTokenString = tokenHandler.WriteToken(accessToken);

            // Генерация refresh token (простой случай: случайная строка)
            var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            // Сохраните refresh token где-нибудь (например, в таблице UserRefreshTokens)
            // Для простоты возвращаем его, но в реальности нужно связать с пользователем

            return Ok(new
            {
                access_token = accessTokenString,
                token_type = "Bearer",
                expires_in = 3600,
                refresh_token = refreshToken
            });
        }
        public class LoginRequest
        {
            public string UsernameOrEmail { get; set; }
            public string Password { get; set; }
        }
        private SecurityKey GetSigningKey()
        {
            // Вариант 1: из настроек OpenIddict (если вы используете development сертификат)
            var options = _serviceProvider.GetRequiredService<IOptions<OpenIddictServerOptions>>();
            var signingKey = options.Value.SigningCredentials.FirstOrDefault()?.Key;
            if (signingKey != null) return signingKey;

            // Вариант 2: вручную создать RSA ключ (только для разработки)
            using var rsa = new RSACryptoServiceProvider(2048);
            return new RsaSecurityKey(rsa.ExportParameters(true));
        }
    }
}
