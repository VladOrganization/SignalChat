using MediatR;
using Microsoft.EntityFrameworkCore;
using SignalChat.Backend.Database;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Chat.GetMessages;

public class GetMessagesHandler(ChatDbContext db) : IRequestHandler<GetMessagesQuery, PagedResult<MessageDto>>
{
    public async Task<PagedResult<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var totalCount = await db.Messages.CountAsync(cancellationToken);
        var items = await db.Messages
            .OrderByDescending(m => m.Time)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new MessageDto(m.Id, m.Text, m.User.UserName, m.Time,m.Images.Select(x=>x.ImageUrl).ToList()))
            .ToListAsync(cancellationToken);

        return new PagedResult<MessageDto>(items, totalCount, request.Page, request.PageSize);
    }
}
