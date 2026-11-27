using SignalChat.Backend.Database.Entities;
using SignalChat.Backend.Database.Entities.Enums;

namespace SignalChat.Backend.Models;

public record MessageDto(Guid Id,Guid DialogId, string Text, string UserName, DateTime Time,List<string> Images);
