using SignalChat.Backend.Database.Entities.Enums;

namespace SignalChat.Backend.Models
{
    public class ReactionCount
    {
        public ReactionEnum ReactionEnum { get; set; }
        public int Count { get; set; }
    }
    public record GetMessageResponse(
        Guid Id,
        string Text,
        string UserName, 
        DateTime Time,
        List<string> Images,
        List<ReactionCount> ReactonCount
        );
}
