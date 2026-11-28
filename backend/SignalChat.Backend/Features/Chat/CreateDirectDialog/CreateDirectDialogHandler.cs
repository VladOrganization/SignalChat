using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SignalChat.Backend.Database;
using SignalChat.Backend.Database.Entities;
using SignalChat.Backend.Database.Entities.Enums;
using SignalChat.Backend.Exceptions;
using SignalChat.Backend.Services;

namespace SignalChat.Backend.Features.Chat.CreateDirectDialog;

public class CreateDirectDialogHandler(ChatDbContext db)
    : IRequestHandler<CreateDirectDialogCommand, Guid>
{
    public async Task<Guid> Handle(CreateDirectDialogCommand request, CancellationToken cancellationToken)
    {
        var partner = await db.Users.AnyAsync(u => u.Id == request.PartnerId,cancellationToken);
        if (!partner)
            throw new NotFoundException("такого пользователя нету");

        var key = DirectKeyBuilder.Create(request.CurrentUserId, request.PartnerId);

        var dialog = await db.Dialogs.FirstOrDefaultAsync(d => d.DirectKey == key,cancellationToken);
        
        if (dialog != null)
            return dialog.Id;

        var newDialog = new Dialog
        {
            Id = Guid.NewGuid(),
            Type = DialogEnum.Direct,
            DirectKey = key,
            CreatedAt = DateTime.UtcNow
        };

        var newDialogParcipantMe = new DialogParticipant
        {
            DialogId = newDialog.Id,
            UserId =  request.CurrentUserId,
            JoinedAt = DateTime.UtcNow,
            LastReadAt = null
        };
        
        var newDialogParcipantPartner = new DialogParticipant
        {
            DialogId = newDialog.Id,
            UserId =  request.PartnerId,
            JoinedAt = DateTime.UtcNow,
            LastReadAt = null
        };

        try
        {
            await db.Dialogs.AddAsync(newDialog, cancellationToken);
            await db.DialogParticipants.AddAsync(newDialogParcipantMe, cancellationToken);
            await db.DialogParticipants.AddAsync(newDialogParcipantPartner,cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            
            var existing = await db.Dialogs
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DirectKey == key, cancellationToken);

            if (existing is null)
                throw;

            return existing.Id;
        }

        return newDialog.Id;
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
        => exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
