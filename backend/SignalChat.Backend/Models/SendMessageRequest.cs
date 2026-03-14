namespace SignalChat.Backend.Models;

public record SendMessageRequest(string Text,List<string>? ImageUrl);
