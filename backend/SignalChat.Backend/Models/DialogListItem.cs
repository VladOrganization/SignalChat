using SignalChat.Backend.Database.Entities.Enums;

namespace SignalChat.Backend.Models;

/// <summary>Краткая информация о диалоге для списка чатов.</summary>
public record DialogListItem(
    Guid DialogId,
    DialogEnum Type,
    string? Title,
    string? LastMessageText,
    DateTime? LastMessageTime,
    int UnreadCount
);
