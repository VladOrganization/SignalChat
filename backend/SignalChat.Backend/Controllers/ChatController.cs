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

[Route("api/[controller]")]
public class ChatController(ISender sender) : ControllerBase
{
    [Authorize]
    [HttpGet("messages")]
    public Task<PagedResult<MessageDto>> GetMessages([FromQuery] GetMessagesQuery query, CancellationToken ct)
        => sender.Send(query, ct);

    private readonly IWebHostEnvironment _env;
    [HttpPost("send-image")]
   
    public async Task<ActionResult> UploadAvatar(IFormFile file)
    {


        if (file.Length == 0)
        {
            return BadRequest("File is empty");
        }

        var avatarsDir = Path.Combine(_env.WebRootPath, "avatars"); //C:\Users\vladi\RiderProjects\AuthSample\AuthSample.Backend\wwwroot\avatars
        Directory.CreateDirectory(avatarsDir);

        var fileName = $"{userId}.webp";
        var filePath = Path.Combine(avatarsDir, fileName);

        using (var image = await Image.LoadAsync(file.OpenReadStream()))
        {
            var size = Math.Min(image.Width, image.Height);
            image.Mutate(x => x
                .Crop(new Rectangle((image.Width - size) / 2, (image.Height - size) / 2, size, size))
                .Resize(500, 500));

            await image.SaveAsWebpAsync(filePath);
        }

        user.AvatarUrl = $"/avatars/{fileName}";
        await _dbContext.SaveChangesAsync();

        return Ok(new { AvatarUrl = user.AvatarUrl });
    }
    [Authorize]
    [HttpPost("messages")]
    public Task<MessageDto> SendMessage([FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedException("Пользователь не авторизован");
        
        return sender.Send(new SendMessageCommand(userId, request.Text), ct);
    }
}
