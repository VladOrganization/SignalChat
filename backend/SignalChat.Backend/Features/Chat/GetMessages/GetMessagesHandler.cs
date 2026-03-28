using MediatR;
using Microsoft.EntityFrameworkCore;
using SignalChat.Backend.Database;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Chat.GetMessages;

public class GetMessagesHandler(ChatDbContext db) : IRequestHandler<GetMessagesQuery, PagedResult<GetMessageResponse>>
{
    public async Task<PagedResult<GetMessageResponse>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var totalCount = await db.Messages.CountAsync(cancellationToken);
        var items = await db.Messages
            .OrderByDescending(m => m.Time)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new GetMessageResponse(m.Id, m.Text, m.User.UserName, m.Time,m.Images.Select(x=>x.ImageUrl).ToList(),
            m.Reaction.GroupBy(x=>x.Emoji).Select(r=>new ReactionCount {ReactionEnum =  r.Key,Count = r.Count() }).ToList()
            ))
            .ToListAsync(cancellationToken);

        // [1, 2, 1, 2, 3, 1]

        return new PagedResult<GetMessageResponse>(items, totalCount, request.Page, request.PageSize);
    }
}
