using SignalChat.Backend.Database.Entities;

namespace SignalChat.Backend.Models;

public record MessageDto(Guid Id, string Text, string UserName, DateTime Time,List<string> Images);
