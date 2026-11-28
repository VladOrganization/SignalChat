using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalChat.Backend.Database;
using SignalChat.Backend.Database.Entities;
using SignalChat.Backend.Database.Entities.Enums;
using SignalChat.Backend.Exceptions;
using SignalChat.Backend.Hubs;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Chat.ReactionMessage
{
    public class ReactionMessageHandler(ChatDbContext db, IHubContext<ChatHub> hubContext)
        : IRequestHandler<ReactionMessageCommand>
    {
        public async Task Handle(ReactionMessageCommand request, CancellationToken cancellationToken)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new UnauthorizedException("Пользователь не найден");

            // Проверка доступа: сообщение существует, и пользователь — участник его диалога.
            var message = await db.Messages.FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken)
            ?? throw new NotFoundException("Сообщение не найдено");

            var dialog = await db.Dialogs.FirstOrDefaultAsync(x => x.Id == message.DialogId, cancellationToken)
            ?? throw new NotFoundException("Диалог не найден");

            if (dialog.Type != DialogEnum.GlobalChat)
            {
                var participant = await db.DialogParticipants.FirstOrDefaultAsync(
                    x => x.DialogId == dialog.Id && x.UserId == user.Id,
                    cancellationToken
                );
                if (participant == null)
                    throw new NotFoundException("Сообщение не найдено");
            }

            var reaction = await db.Reactions.FirstOrDefaultAsync(u => u.UserId == user.Id&&u.MessageId == request.MessageId,cancellationToken);

            
            var id = Guid.NewGuid();
            if (reaction == null) {
                db.Reactions.Add(new Reaction
                {
                    MessageId = request.MessageId,
                    Emoji = request.Reaction,
                    Id = id,
                    UserId = request.UserId
                });
            }
            else if (reaction.Emoji != request.Reaction)
            {
                reaction.Emoji = request.Reaction;
            }
            else if (reaction.Emoji == request.Reaction)
            {
                throw new ConflictException("Вы уже ставили эту реакцию на это сообшение");
            }

            await db.SaveChangesAsync(cancellationToken);

            // В событии передаём диалог и сообщение, чтобы клиент мог отфильтровать чужое
            // и не перезагружать весь список сообщений.
            var payload = new
            {
                dialogId = dialog.Id,
                messageId = request.MessageId,
                reaction = request.Reaction
            };

            if (dialog.Type == DialogEnum.GlobalChat)
            {
                await hubContext.Clients.All.SendAsync("RecieveReaction", payload, cancellationToken);
                return;
            }

            var participants = await db.DialogParticipants
                .Where(x => x.DialogId == dialog.Id)
                .Select(x => x.UserId)
                .ToListAsync(cancellationToken);

            // Реакция уже сохранена, поэтому при пустом списке просто не рассылаем.
            if (participants.Count == 0)
                return;

            await hubContext.Clients
                .Users(participants)
                .SendAsync("RecieveReaction", payload, cancellationToken);
        }
    }
}

