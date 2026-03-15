using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalChat.Backend.Database;
using SignalChat.Backend.Database.Entities;
using SignalChat.Backend.Exceptions;
using SignalChat.Backend.Hubs;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Chat.SendMessage;

public class SendMessageHandler(ChatDbContext db, IHubContext<ChatHub> hubContext)
    : IRequestHandler<SendMessageCommand, MessageDto>
{
    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new UnauthorizedException("Пользователь не найден");
        
        var id = Guid.NewGuid();
        var images = request.Images.Select(x => new Image {ImageUrl = x, MessageId = id }).ToList();
        
        var message = new Message
        {
            Id = id,
            Text = request.Text,
            UserId = request.UserId,
            Time = DateTime.UtcNow,
            Images = images
          
        };

        db.Messages.Add(message);
        await db.SaveChangesAsync(cancellationToken);

        var dto = new MessageDto(message.Id,
            message.Text,
            user.UserName,
            message.Time,
            message.Images.Select(x=>x.ImageUrl).ToList(),
            request.Reactions
            );

        await hubContext.Clients.All.SendAsync("ReceiveMessage", dto, cancellationToken);

        return dto;
    }
}
