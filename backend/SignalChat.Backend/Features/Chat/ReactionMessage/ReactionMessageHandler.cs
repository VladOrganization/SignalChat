using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalChat.Backend.Database;
using SignalChat.Backend.Database.Entities;
using SignalChat.Backend.Exceptions;
using SignalChat.Backend.Hubs;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Chat.ReactionMessage
{
    public class ReactionMessageHandler(ChatDbContext db, IHubContext<ChatHub> hubContext)
        : IRequestHandler<ReactionMessageCommand, MessageDto>
    {
        public async Task<MessageDto> Handle(ReactionMessageCommand request, CancellationToken cancellationToken)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new UnauthorizedException("Пользователь не найден");

            var id = Guid.NewGuid();

            var reaction = new Message
            {
                Id=request.MessageId,
                Reaction = request.Reactions,
                UserId=user.Id,

            };

            db.Messages.Update(reaction);

            db.SaveChanges();

            

           
            await hubContext.Clients.All.SendAsync("ReceiveMessage", reaction, cancellationToken);

            return re;

            
        }
    }
}

