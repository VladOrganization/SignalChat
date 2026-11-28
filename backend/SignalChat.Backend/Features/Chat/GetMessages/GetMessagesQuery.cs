using MediatR;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Chat.GetMessages;

public record GetMessagesQuery(Guid DialogId,string CurrentUserIde,int Page = 1, int PageSize = 50) 
    : IRequest<PagedResult<GetMessageResponse>>;
