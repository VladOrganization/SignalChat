using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignalChat.Backend.Exceptions;
using SignalChat.Backend.Features.Chat.GetMessages;
using SignalChat.Backend.Features.Chat.ReactionMessage;
using SignalChat.Backend.Features.Chat.SendMessage;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Controllers;

[ApiController]

[Route("api/[controller]")]
public class ChatController(ISender sender) : ControllerBase
{
    [Authorize]
    [HttpGet("messages")]
    public Task<PagedResult<MessageDto>> GetMessages([FromQuery] GetMessagesQuery query, CancellationToken ct)
        => sender.Send(query, ct);
    
    [Authorize]
    [HttpPost("messages")]
    public Task<MessageDto> SendMessage([FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedException("Пользователь не авторизован");
        
        return sender.Send(new SendMessageCommand(userId, request.Text,request.ImageUrl,request.Reactions), ct);
    }

    [Authorize]
    [HttpPut("reactions")]
    public Task<MessageDto> ReactionMessage([FromBody] ReactionMessageRequest request, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedException("Пользователь не авторизован");
        
        return sender.Send(new ReactionMessageCommand(userId, request.MessageId,request.Reactions), ct);
    }
}
