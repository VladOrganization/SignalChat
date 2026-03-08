using MediatR;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Chat.GetMessages;

public record GetMessagesQuery(int Page = 1, int PageSize = 50) : IRequest<PagedResult<MessageDto>>;
