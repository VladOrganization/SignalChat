using SignalChat.Backend.Database.Entities.Enums;

namespace SignalChat.Backend.Models
{
    public record ReactionCount(ReactionEnum ReactionEnum,int Count);
    
    public record GetMessageResponse(
        Guid Id,
        string Text,
        string UserName, 
        DateTime Time,
        List<string> Images,
        Guid DialogId,
        string UserId,
        List<ReactionCount> Reactions
        );
}
