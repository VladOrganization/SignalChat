using Microsoft.AspNetCore.Identity;

namespace SignalChat.Backend.Database.Entities;

public class User:IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public string? ImageUrl { get; set; }
    public List<Reaction> Reaction { get; set; }
}