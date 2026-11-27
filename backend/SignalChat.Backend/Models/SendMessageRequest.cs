using SignalChat.Backend.Database.Entities.Enums;

namespace SignalChat.Backend.Models;

public record SendMessageRequest(Guid DialogId, string Text, List<string>? ImageUrl);
