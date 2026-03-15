using MediatR;
using SignalChat.Backend.Database.Entities;
using SignalChat.Backend.Database.Entities.Enums;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Chat.ReactionMessage
{
    public record ReactionMessageCommand(Guid MessageId, Guid UserId, List<ReactionEnum> Reactions) : IRequest<ReactionEnum>;
}

    
 