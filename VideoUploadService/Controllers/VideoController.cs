using Microsoft.AspNetCore.Mvc;

namespace VideoUploadService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoController : ControllerBase
{
    private readonly string _uploadFolder = Path.Combine(
        Directory.GetCurrentDirectory(),
        "wwwroot",
        "uploads"
    );

    public VideoController()
    {
        if (!Directory.Exists(_uploadFolder))
        {
            Directory.CreateDirectory(_uploadFolder);
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadVideo([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var filePath = Path.Combine(_uploadFolder, file.FileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new { FileName = file.FileName });
    }

    [HttpGet("list")]
    public IActionResult ListVideos()
    {
        var files = Directory.GetFiles(_uploadFolder).Select(Path.GetFileName).ToList();

        return Ok(files);
    }
}
