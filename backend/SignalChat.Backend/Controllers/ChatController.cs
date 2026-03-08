using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignalChat.Backend.Exceptions;
using SignalChat.Backend.Features.Chat.GetMessages;
using SignalChat.Backend.Features.Chat.SendMessage;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ChatController(ISender sender) : ControllerBase
{
    [HttpGet("messages")]
    public Task<PagedResult<MessageDto>> GetMessages([FromQuery] GetMessagesQuery query, CancellationToken ct)
        => sender.Send(query, ct);

    [HttpPost("messages")]
    public Task<MessageDto> SendMessage([FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedException("Пользователь не авторизован");

        return sender.Send(new SendMessageCommand(userId, request.Text), ct);
    }
}
