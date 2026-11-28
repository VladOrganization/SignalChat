using MediatR;
using Microsoft.EntityFrameworkCore;
using SignalChat.Backend.Database;
using SignalChat.Backend.Database.Entities.Enums;
using SignalChat.Backend.Exceptions;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Chat.GetDialogs;

public class GetDialogsHandler(ChatDbContext db)
    : IRequestHandler<GetDialogsQuery, List<DialogListItem>>
{
    public async Task<List<DialogListItem>> Handle(
        GetDialogsQuery request,
        CancellationToken cancellationToken)
    {
        var userExists = await db.Users.AnyAsync(u => u.Id == request.CurrentUserId,
            cancellationToken);
        if (!userExists)
            throw new UnauthorizedException("Пользователь не найден");

        //dialog username + last message + unread count
        var dialogs = await db.Dialogs
            .Include(d => d.Participants)
            .ThenInclude(p => p.User)
            .Include(d => d.Messages)
            .Where(d => d.Participants.Any(p => p.UserId == request.CurrentUserId) || !d.Participants.Any())
            .Select(d => new
            {
                DialogId = d.Id,
                DialogType = d.Type,
                Title = d.Participants.FirstOrDefault(p => p.UserId != request.CurrentUserId).User.UserName,
                UnreadCount = d.Messages.Count(m =>
                    m.Time > d.Participants.FirstOrDefault(p => p.UserId != request.CurrentUserId).LastReadAt),
                LastMessage = d.Messages.OrderBy(m => m.Time).LastOrDefault()
            })
            .ToListAsync(cancellationToken);

        return dialogs.Select(d => new DialogListItem(d.DialogId,
            d.DialogType,
            d.DialogType == DialogEnum.Direct ? d.Title : GlobalDialog.Title,
            d.LastMessage.Text,
            d.LastMessage.Time,
            d.UnreadCount))
            .ToList();
    }
}