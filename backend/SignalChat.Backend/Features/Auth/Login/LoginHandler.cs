using MediatR;
using Microsoft.EntityFrameworkCore;
using SignalChat.Backend.Database;
using SignalChat.Backend.Exceptions;
using SignalChat.Backend.Models;
using SignalChat.Backend.Services;

namespace SignalChat.Backend.Features.Auth.Login;

public class LoginHandler(ChatDbContext db, TokenService tokenService)
    : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Code == request.Code, cancellationToken)
            ?? throw new UnauthorizedException("Неверный код");

        user.RefreshToken = tokenService.GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.Add(tokenService.RefreshTokenExpiry);

        await db.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Code = user.Code,
            AccessToken = tokenService.GenerateAccessToken(user),
            RefreshToken = user.RefreshToken
        };
    }
}
