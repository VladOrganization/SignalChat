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



            var reaction = new Message
            {
                Id=request.MessageId,
                Reaction = request.Reactions.Select(x=> new Reaction { Reactions = x}).ToList(),
                UserId=request.UserId,

            };

            db.Messages.Update(reaction);

            await db.SaveChangesAsync(cancellationToken);




            await hubContext.Clients.All.SendAsync("ReceiveMessage", reaction, cancellationToken);

            return new MessageDto(reaction.Id,
                reaction.Text,
                user.UserName,
                reaction.Time,
                reaction.Images.Select(x => x.ImageUrl).ToList(),
                request.Reactions.Select(x => x).ToList()
            );


        }
    }
}

