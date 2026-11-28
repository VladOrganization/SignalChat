using MediatR;

namespace SignalChat.Backend.Features.Chat.CreateDirectDialog;

public record CreateDirectDialogCommand(string CurrentUserId, string PartnerId) : IRequest<Guid>;
