using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web;

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
    public async Task<ActionResult<List<string>>> UploadAvatar(List<IFormFile> file)
    {
        string[] availableFormats = [".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".webp", ".avif", ".gif"];
        List<string> paths = new List<string>();
        if (file.Count > 9) {
            return BadRequest("many files uploaded");
        }
        foreach (var format in file)
        {
            if (format.Length == 0)
            {
                return BadRequest("File is empty");
            }
            
            if (!availableFormats.Contains(Path.GetExtension(format.FileName).ToLower()))
            {
                return BadRequest("File format not supported");
            }

            if (format.Length > 20_000_000)
            {
                return BadRequest("File is too large");
            }
        }
        var avatarsDir = Path.Combine(_env.WebRootPath, "images");

        foreach (var item in file)
        {
            var fileName = $"{Guid.CreateVersion7()}.webp";
            var filePath = Path.Combine(avatarsDir, fileName);
            using var image = await Image.LoadAsync(item.OpenReadStream());

            image.Mutate(x => x
                .Resize(600, 0)
            );
            await image.SaveAsWebpAsync(filePath);
            paths.Add(Path.Combine("images", fileName));
        }

        return paths;
        
    }
}