using MediatR;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Chat.SendMessage;

public record SendMessageCommand(Guid UserId, string Text, string? ImageUrl) : IRequest<MessageDto>;
