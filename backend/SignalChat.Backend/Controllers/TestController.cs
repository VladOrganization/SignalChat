using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OpenIddict.Server;
using SignalChat.Backend.Database.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SignalChat.Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        // Ваши данные EmailJS
        private const string EmailJsServiceId = "service_ael9hh5";
        private const string EmailJsTemplateId = "template_ivkqa3f";
        private const string EmailJsPublicKey = "KQ8zhAP6KrVVGVEOr";

        public TestController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public class LoginRequest
        {
            public string UsernameOrEmail { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Валидация входных данных
            if (request == null || string.IsNullOrWhiteSpace(request.UsernameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Username/email and password are required." });

            // 2. Поиск пользователя (сначала по username, потом по email, если похоже)
            var user = await _userManager.FindByNameAsync(request.UsernameOrEmail);
            if (user == null && request.UsernameOrEmail.Contains('@'))
                user = await _userManager.FindByEmailAsync(request.UsernameOrEmail);

            if (user == null)
                return Unauthorized(new { error = "Invalid username/email or password." });

            // 3. Проверка пароля
            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!signInResult.Succeeded)
                return Unauthorized(new { error = "Invalid username/email or password." });

            // 4. Фоново отправляем email-уведомление (не блокируем ответ)
            _ = Task.Run(async () =>
            {
                try
                {
                    var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
                    await SendLoginEmailAsync(user.Email, user.UserName, ip);
                }
                catch (Exception ex)
                {
                    // Логируйте ошибку, если нужен логгер
                    Console.WriteLine($"Email sending failed: {ex.Message}");
                }
            });

            // 5. Генерация JWT access token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = GetSigningKey();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "")
            };

            // Добавляем роли, если они используются
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"] ?? "https://localhost:5001",
                Audience = _configuration["Jwt:Audience"] ?? "SignalChat.Api",
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
            };

            var accessToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessTokenString = tokenHandler.WriteToken(accessToken);

            // 6. Генерация refresh token (случайная строка)
            var refreshToken = GenerateRefreshToken();

            // В реальном приложении refresh token нужно сохранить в БД, связав с пользователем
            // Например: await _refreshTokenRepository.SaveAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7));

            // 7. Возвращаем ответ
            return Ok(new
            {
                access_token = accessTokenString,
                token_type = "Bearer",
                expires_in = 3600,
                refresh_token = refreshToken
            });
        }

        // ========== Вспомогательные методы ==========

        private SecurityKey GetSigningKey()
        {
            // Пытаемся взять ключ подписи из настроек OpenIddict (если они сконфигурированы)
            var openIddictOptions = HttpContext.RequestServices.GetService(typeof(Microsoft.Extensions.Options.IOptions<OpenIddictServerOptions>))
                as Microsoft.Extensions.Options.IOptions<OpenIddictServerOptions>;
            if (openIddictOptions?.Value?.SigningCredentials?.Count > 0)
                return openIddictOptions.Value.SigningCredentials[0].Key;

            // Запасной вариант: создаём новый RSA ключ (только для разработки)
            using var rsa = new RSACryptoServiceProvider(2048);
            return new RsaSecurityKey(rsa.ExportParameters(true));
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private async Task<bool> SendLoginEmailAsync(string userEmail, string userName, string ipAddress)
        {
            if (string.IsNullOrEmpty(userEmail))
                return false;

            using var client = _httpClientFactory.CreateClient();
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var messageText = $"Вход в аккаунт выполнен {time}\nIP: {ipAddress ?? "неизвестно"}";

            var payload = new
            {
                service_id = EmailJsServiceId,
                template_id = EmailJsTemplateId,
                user_id = EmailJsPublicKey,
                template_params = new
                {
                    to_email = userEmail,           // 👈 КЛЮЧЕВОЙ ПАРАМЕТР
                    from_name = userName ?? userEmail,
                    from_email = userEmail,
                    message = messageText
                }
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://api.emailjs.com/api/v1.0/email/send", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"EmailJS error: {response.StatusCode} - {responseBody}");
                    return false;
                }
                Console.WriteLine($"✅ Email sent to {userEmail}: {responseBody}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EmailJS exception: {ex.Message}");
                return false;
            }
        }
    }
}