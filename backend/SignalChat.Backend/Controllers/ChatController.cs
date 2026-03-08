using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignalChat.Backend.Exceptions;
using SignalChat.Backend.Features.Chat.GetMessages;
using SignalChat.Backend.Features.Chat.SendMessage;
using SignalChat.Backend.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace SignalChat.Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ChatController(ISender sender) : ControllerBase
{
    [HttpGet("messages")]
    public Task<PagedResult<MessageDto>> GetMessages([FromQuery] GetMessagesQuery query, CancellationToken ct)
        => sender.Send(query, ct);

    private readonly IWebHostEnvironment _env;
    [HttpPost("send-image")]
    public async Task<IActionResult> SendInage(IFormFile file)
    {
        if (file.Length == 0)
        {
            return BadRequest("file is not send");
        }
        var uploadsFolder = Path.Combine(_env.WebRootPath, "images");
        Directory.CreateDirectory(uploadsFolder);
        
        var fileName = $"{Guid.NewGuid()}.jpg";
        var filePath = Path.Combine(uploadsFolder, fileName);
        using (var stream = file.OpenReadStream())
        using (var image = await Image.LoadAsync(stream))
        {
            if (image.Width > 800)
            {
                image.Mutate(x => x.Resize(800, 800));
            }

            var encoder = new JpegEncoder { Quality = 75 };
            await image.SaveAsync(filePath, encoder);
        }

        return Ok(new { path = $"/images/{fileName}" });
    }
    [HttpPost("messages")]
    public Task<MessageDto> SendMessage([FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedException("Пользователь не авторизован");
        
        return sender.Send(new SendMessageCommand(userId, request.Text), ct);
    }
}
