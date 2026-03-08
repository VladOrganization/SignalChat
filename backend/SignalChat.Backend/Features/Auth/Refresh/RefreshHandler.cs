using MediatR;
using Microsoft.EntityFrameworkCore;
using SignalChat.Backend.Database;
using SignalChat.Backend.Exceptions;
using SignalChat.Backend.Models;
using SignalChat.Backend.Services;

namespace SignalChat.Backend.Features.Auth.Refresh;

public class RefreshHandler(ChatDbContext db, TokenService tokenService)
    : IRequestHandler<RefreshCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(
                u => u.RefreshToken == request.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedException("Refresh token не найден");

        if (user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token истёк");

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
