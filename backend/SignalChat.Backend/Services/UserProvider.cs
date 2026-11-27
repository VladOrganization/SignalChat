using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using OpenIddict.Abstractions;

namespace SignalChat.Backend.Services;

public class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => connection.User?.FindFirstValue(OpenIddictConstants.Claims.Subject);
}