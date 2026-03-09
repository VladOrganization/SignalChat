using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace SignalChat.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public ImageController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("save")]
    public async Task<ActionResult<string>> UploadAvatar(IFormFile file)
    {
        if (file.Length == 0)
        {
            return BadRequest("File is empty");
        }

        string[] availableFormats = [".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".webp", ".avif", ".gif"];

        if (!availableFormats.Contains(Path.GetExtension(file.FileName).ToLower()))
        {
            return BadRequest("File format not supported");
        }

        if (file.Length > 20_000_000)
        {
            return BadRequest("File is too large");
        }

        var avatarsDir = Path.Combine(_env.WebRootPath, "images");

        var fileName = $"{Guid.CreateVersion7()}.webp";
        var filePath = Path.Combine(avatarsDir, fileName);

        using var image = await Image.LoadAsync(file.OpenReadStream());
        
        image.Mutate(x => x
            .Resize(600, 0)
        );

        await image.SaveAsWebpAsync(filePath);

        return filePath;
    }
}