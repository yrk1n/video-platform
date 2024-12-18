using Microsoft.AspNetCore.Mvc;
using VideoUploadService.Services;

namespace VideoUploadService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoController : ControllerBase
{
    private readonly string _uploadFolder;
    private readonly VideoProcessingService _processingService;

    public VideoController(VideoProcessingService processingService)
    {
        _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _processingService = processingService;

        if (!Directory.Exists(_uploadFolder))
            Directory.CreateDirectory(_uploadFolder);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadVideo([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(_uploadFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Queue for processing
        await _processingService.QueueVideoForProcessing(new VideoProcessingJob
        {
            OriginalFilePath = filePath,
            FileName = fileName
        });

        return Ok(new { FileName = fileName });
    }

    [HttpGet("versions/{fileName}")]
    public IActionResult GetVideoVersions(string fileName)
    {
        var baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "processed");
        var videoFolder = Path.Combine(baseFolder, Path.GetFileNameWithoutExtension(fileName));

        if (!Directory.Exists(videoFolder))
            return NotFound();

        var versions = new
        {
            FileName = fileName,
            Versions = new[]
            {
                new { Resolution = "720p", Url = $"/processed/{Path.GetFileNameWithoutExtension(fileName)}/720p.mp4" },
                new { Resolution = "480p", Url = $"/processed/{Path.GetFileNameWithoutExtension(fileName)}/480p.mp4" }
            }
        };

        return Ok(versions);
    }

    [HttpGet("list")]
    public IActionResult ListVideos()
    {
        var uploadedFiles = Directory.GetFiles(_uploadFolder)
            .Select(Path.GetFileName)
            .ToList();

        var processedFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "processed");
        var videos = new List<object>();

        foreach (var file in uploadedFiles)
        {
            var processedPath = Path.Combine(processedFolder, Path.GetFileNameWithoutExtension(file));
            var versions = new List<string>();

            if (Directory.Exists(processedPath))
            {
                if (System.IO.File.Exists(Path.Combine(processedPath, "720p.mp4")))
                    versions.Add("720p");
                if (System.IO.File.Exists(Path.Combine(processedPath, "480p.mp4")))
                    versions.Add("480p");
            }

            videos.Add(new
            {
                fileName = file,
                originalUrl = $"/uploads/{file}",
                processedVersions = versions,
                isProcessing = versions.Count == 0
            });
        }

        return Ok(videos);
    }
}