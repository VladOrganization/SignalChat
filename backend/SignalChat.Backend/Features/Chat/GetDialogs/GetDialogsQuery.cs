using MediatR;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Chat.GetDialogs;

public record GetDialogsQuery(string CurrentUserId) : IRequest<List<DialogListItem>>;
