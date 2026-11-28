using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using SignalChat.Backend.Database;
using SignalChat.Backend.Exceptions;
using SignalChat.Backend.Features.Chat.CreateDirectDialog;
using SignalChat.Backend.Features.Chat.GetDialogs;
using SignalChat.Backend.Features.Chat.GetMessages;
using SignalChat.Backend.Features.Chat.ReactionMessage;
using SignalChat.Backend.Features.Chat.SendMessage;
using SignalChat.Backend.Features.User.SearchUsers;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Controllers;

[ApiController]

[Route("api/[controller]")]
public class ChatController(ISender sender) : ControllerBase
{
    public record CreateDirectDialogRequest(string UserId);

    [Authorize]
    [HttpGet("users")]
    public Task<List<UserListItem>> SearchUsers([FromQuery]string? search,CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(OpenIddictConstants.Claims.Subject);
        if (userIdStr is null)
            throw new UnauthorizedException("пользователь не найден");
        return  sender.Send(new SearchUsersQuery(userIdStr,search),ct);
    }
    
    [Authorize]
    [HttpGet("global")]
    public IActionResult GetGlobalDialog() => Ok(new { dialogId = GlobalDialog.Id });

    [Authorize]
    [HttpGet("dialogs")]
    public Task<List<DialogListItem>> GetDialogs(CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(OpenIddictConstants.Claims.Subject);
        if (userIdStr is null)
            throw new UnauthorizedException("Пользователь не авторизован");

        return sender.Send(new GetDialogsQuery(userIdStr), ct);
    }

    [Authorize]
    [HttpPost("direct")]
    public Task<Guid> CreateDirectDialog(
        [FromBody] CreateDirectDialogRequest request,
        CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(OpenIddictConstants.Claims.Subject);
        if (userIdStr is null)
            throw new UnauthorizedException("Пользователь не авторизован");

        return sender.Send(new CreateDirectDialogCommand(userIdStr, request.UserId), ct);
    }
    [Authorize]
    [HttpGet("messages")]
    public Task<PagedResult<GetMessageResponse>> GetMessages(
        [FromQuery] Guid dialogId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(OpenIddictConstants.Claims.Subject);
        if (userIdStr is null)
            throw new UnauthorizedException("Пользователь не авторизован");

        return sender.Send(
            new GetMessagesQuery(dialogId, userIdStr,page ?? 1, pageSize ?? 50), ct);
    }
    
    [Authorize]
    [HttpPost("messages")]
    public Task<MessageDto> SendMessage([FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(OpenIddictConstants.Claims.Subject);
        if (userIdStr is null)
            throw new UnauthorizedException("Пользователь не авторизован");
        
        return sender.Send(new SendMessageCommand(userIdStr, request.Text, request.ImageUrl, request.DialogId), ct);
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
