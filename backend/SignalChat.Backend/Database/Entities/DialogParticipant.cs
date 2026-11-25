namespace SignalChat.Backend.Database.Entities;

public class DialogParticipant
{
    public Guid DialogId { get; set; }
    public Dialog Dialog { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public DateTime JoinedAt { get; set; }
    
    public DateTime? LastReadAt { get; set; }
}