using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
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
    public Task<PagedResult<GetMessageResponse>> GetMessages([FromQuery] GetMessagesQuery query, CancellationToken ct)
        => sender.Send(query, ct);
    
    [Authorize]
    [HttpPost("messages")]
    public Task<MessageDto> SendMessage([FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(OpenIddictConstants.Claims.Subject);
        if (userIdStr is null)
            throw new UnauthorizedException("Пользователь не авторизован");
        
        return sender.Send(new SendMessageCommand(userIdStr, request.Text,request.ImageUrl), ct);
    }

    [Authorize]
    [HttpPost("reactions")]
    public Task ReactionMessage([FromBody] ReactionMessageRequest request, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(OpenIddictConstants.Claims.Subject);
        if (userIdStr is null)
            throw new UnauthorizedException("Пользователь не авторизован");
        
        return sender.Send(new ReactionMessageCommand(request.MessageId,userIdStr, request.Reaction), ct);
    }
}
