namespace SignalChat.Backend.Database.Entities;

public class User
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Code { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public string? ImageUrl { get; set; }

    public List<Reaction> Reaction { get; set; }
}