using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SignalChat.Backend.Database.Entities;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SignalChat.Backend.Controllers
{
    [ApiController]
    [Route("connect")]
    public class AuthorizationController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthorizationController(UserManager<User> userManager,
                                       SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpPost("token")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();

            if (request.IsPasswordGrantType())
            {
                var user = await _userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    return Problem("Invalid username or password.", statusCode: 400);
                }

                

                // Создаем ClaimsPrincipal, который будет подписан
                var principal = await _signInManager.CreateUserPrincipalAsync(user);

                // 1. Добавляем кастомные клеймы (утверждения), которых нет в CreateUserPrincipalAsync
                // Например, добавим "user_id" и все роли пользователя
                principal.SetClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());
                principal.SetClaim(OpenIddictConstants.Claims.Username, user.UserName);

                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    principal.SetClaim(OpenIddictConstants.Claims.Role, role);
                }

                // 2. ГЛАВНОЕ: Указываем, какие клеймы и в какие токены добавлять
                // В этом примере мы разрешаем добавлять все клеймы в Access Token
                // Используем рекомендованный OpenIddict подход через делегат
                principal.SetDestinations(claim => claim.Type switch
                {
                    // Можно гибко настраивать, какие клеймы в какие токены попадают
                    // Например, email может идти и в Access, и в Identity токен
                    OpenIddictConstants.Claims.Email when claim.Subject.HasScope(Scopes.Email) =>
                        new[] { OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken },

                    // А roles — только в Access Token
                    OpenIddictConstants.Claims.Role =>
                        new[] { OpenIddictConstants.Destinations.AccessToken },

                    // Все остальные клеймы добавляем только в Access Token
                    _ => new[] { OpenIddictConstants.Destinations.AccessToken }
                });

                // Возвращаем токен(ы)
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            return BadRequest(new { error = "unsupported_grant_type", error_description = "The specified grant type is not supported." });
        }
    }
}
