namespace SignalChat.Backend.Models;

public class AuthResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
