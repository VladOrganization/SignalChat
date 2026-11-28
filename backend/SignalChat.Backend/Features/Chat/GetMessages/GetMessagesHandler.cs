using MediatR;
using Microsoft.EntityFrameworkCore;
using SignalChat.Backend.Database;
using SignalChat.Backend.Database.Entities.Enums;
using SignalChat.Backend.Exceptions;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Chat.GetMessages;

public class GetMessagesHandler(ChatDbContext db) : IRequestHandler<GetMessagesQuery, PagedResult<GetMessageResponse>>
{
    public async Task<PagedResult<GetMessageResponse>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.CurrentUserIde, cancellationToken)
                   ?? throw new UnauthorizedException("Пользователь не найден");
        var dialog = await db.Dialogs.FirstOrDefaultAsync(x => x.Id == request.DialogId, cancellationToken)
                     ?? throw new NotFoundException("Такого типа чатов не сушествует");
        if (dialog.Type != DialogEnum.GlobalChat)
        {
            var parcipants = await db.DialogParticipants
                .FirstOrDefaultAsync(
                    x => x.DialogId == request.DialogId 
                         && x.UserId == user.Id, cancellationToken);
            if (parcipants == null)
            throw new NotFoundException("сообшений в таком диалоге нету");
            
            
        }
        
        var totalCount = await db.Messages.CountAsync(m => m.DialogId == request.DialogId,cancellationToken);
        var items = await db.Messages
            .Where(m => m.DialogId == request.DialogId)
            .OrderByDescending(m => m.Time)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new GetMessageResponse(
                m.Id,
                m.Text,
                m.User.UserName,
                m.Time,
                m.Images.Select(x => x.ImageUrl).ToList(),
                m.DialogId,    
                m.UserId,     
                m.Reaction.GroupBy(x => x.Emoji).Select(r => new ReactionCount(r.Key, r.Count())).ToList()
            ))
            .ToListAsync(cancellationToken);

        

        return new PagedResult<GetMessageResponse>(items, totalCount, request.Page, request.PageSize);
    }
}
