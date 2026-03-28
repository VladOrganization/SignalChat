using SignalChat.Backend.Database.Entities;
using SignalChat.Backend.Database.Entities.Enums;

namespace SignalChat.Backend.Controllers
{
    public record ReactionMessageResponse(Guid MessageId, ReactionEnum Reactions);
    
}