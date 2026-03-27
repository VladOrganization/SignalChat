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
            var userInfo = await db.Reactions.FirstOrDefaultAsync(u => u.UserId == user.Id&&u.MessageId == request.MessageId,cancellationToken);
            Console.WriteLine(userInfo);
            if (userInfo.Emoji == request.Reaction)
            {
                throw new Exception("Вы уже ставили эту реакцию на это сообшение");
            }
            if (userInfo.Emoji != request.Reaction)
            {
                db.Reactions.Update(new Reaction
                {
                    MessageId = request.MessageId,
                    Emoji = request.Reaction,
                    Id = Guid.NewGuid(),
                });
            }


            await db.SaveChangesAsync(cancellationToken);

            await hubContext.Clients.All.SendAsync("RecieveReaction", request.Reaction, cancellationToken);
        }
    }
}

