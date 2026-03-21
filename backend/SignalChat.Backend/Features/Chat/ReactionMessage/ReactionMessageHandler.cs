using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalChat.Backend.Database;
using SignalChat.Backend.Database.Entities;
using SignalChat.Backend.Database.Entities.Enums;
using SignalChat.Backend.Exceptions;
using SignalChat.Backend.Hubs;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Chat.ReactionMessage
{
    public class ReactionMessageHandler(ChatDbContext db, IHubContext<ChatHub> hubContext)
        : IRequestHandler<ReactionMessageCommand>
    {
        public async Task Handle(ReactionMessageCommand request, CancellationToken cancellationToken)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new UnauthorizedException("Пользователь не найден");

            db.Reactions.Add(new Reaction {
                MessageId = request.MessageId,
                Emoji = request.Reaction,
                Id = Guid.NewGuid(),
            });

            
           
           

            await db.SaveChangesAsync(cancellationToken);

            await hubContext.Clients.All.SendAsync("RecieveReaction", request.Reaction, cancellationToken);
        }
    }
}

