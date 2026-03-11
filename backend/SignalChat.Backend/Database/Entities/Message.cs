namespace SignalChat.Backend.Database.Entities;

public class Message
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public DateTime Time { get; set; }
    public string? ImageUrl { get; internal set; }
}