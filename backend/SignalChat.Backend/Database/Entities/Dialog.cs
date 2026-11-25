using SignalChat.Backend.Database.Entities.Enums;

namespace SignalChat.Backend.Database.Entities;

public class Dialog
{
    public Guid Id { get; set; }
    public DialogEnum Type { get; set; }
    public string? DirectKey { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DialogParticipant> Participants { get; set; } = [];
    public List<Message> Messages { get; set; } = [];
}