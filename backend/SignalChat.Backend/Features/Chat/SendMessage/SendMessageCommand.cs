using MediatR;
using SignalChat.Backend.Database.Entities;
using SignalChat.Backend.Database.Entities.Enums;
using SignalChat.Backend.Models;


namespace SignalChat.Backend.Features.Chat.SendMessage;

public record SendMessageCommand(string UserId, string Text, List<string>? Images) : IRequest<MessageDto>;
