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
        : IRequestHandler<ReactionMessageCommand, ReactionEnum>
    {
        public async Task<ReactionEnum> Handle(ReactionMessageCommand request, CancellationToken cancellationToken)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
           ?? throw new UnauthorizedException("Пользователь не найден");



            var reaction = new Reaction
            {
                Id=request.MessageId,
                Reactions = request.Reactions.FirstOrDefault(),
                MessageId = request.MessageId

            };
            
            db.Reactions.Update(reaction);

            await db.SaveChangesAsync(cancellationToken);




            await hubContext.Clients.All.SendAsync("ReceiveMessage", reaction, cancellationToken);

            return reaction.Reactions;


        }
    }
}

