using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalChat.Backend.Database;
using SignalChat.Backend.Database.Entities;
using SignalChat.Backend.Database.Entities.Enums;
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
        var dialog = await db.Dialogs.FirstOrDefaultAsync(x => x.Id == request.DialogId, cancellationToken)
            ?? throw new NotFoundException("Такого типа чатов не сушествует");

        
        
        var id = Guid.NewGuid();
        var images = request.Images?.Select(x => new Image {ImageUrl = x, MessageId = id }).ToList();
        
        var message = new Message
        {
            Id = id,
            Text = request.Text,
            UserId = request.UserId,
            Time = DateTime.UtcNow,
            Images = images ?? [],
            DialogId = request.DialogId
        };
        
        db.Messages.Add(message);
                
        var msg = new MessageDto(message.Id,
            message.DialogId,
            message.Text,
            user.UserName,
            message.Time,
            message.Images.Select(x=>x.ImageUrl).ToList()
        );
        await db.SaveChangesAsync(cancellationToken);
        
        if (dialog.Type == DialogEnum.GlobalChat)
        {
            await hubContext.Clients.All.SendAsync("ReceiveMessage", msg, cancellationToken);
        }
        else
        {
            var participants = await db.DialogParticipants
                .Where(x => x.DialogId == dialog.Id)
                .Select(x=>x.UserId)
                .ToListAsync(cancellationToken);

            if (participants.Count == 0)
            {
                throw new NotFoundException("пользователей нет в чате");
            }

            await hubContext.Clients
                .Users(participants)
                .SendAsync("ReceiveMessage", msg, cancellationToken);

        }
        
        return msg;
    }
}
