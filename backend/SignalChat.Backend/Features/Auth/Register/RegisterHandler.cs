using MediatR;
using Microsoft.EntityFrameworkCore;
using SignalChat.Backend.Database;
using SignalChat.Backend.Database.Entities;
using SignalChat.Backend.Exceptions;
using SignalChat.Backend.Models;
using SignalChat.Backend.Services;

namespace SignalChat.Backend.Features.Auth.Register;

public class RegisterHandler(ChatDbContext db, TokenService tokenService)
    : IRequestHandler<RegisterCommand, AuthResponse>
{
    private const string CodeChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(u => u.UserName == request.UserName, cancellationToken))
            throw new ConflictException("Имя уже занято");

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            Code = GenerateCode()
        };

        db.Users.Add(user);

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

    private static string GenerateCode() =>
        new(Enumerable.Range(0, 6)
            .Select(_ => CodeChars[Random.Shared.Next(CodeChars.Length)])
            .ToArray());
}
