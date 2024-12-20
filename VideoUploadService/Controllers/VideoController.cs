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
        var fileExtension = Path.GetExtension(fileName);

        if (!Directory.Exists(videoFolder))
            return NotFound();

        var versions = new
        {
            FileName = fileName,
            Versions = new[]
            {
                new { Resolution = "720p", Url = $"/processed/{Path.GetFileNameWithoutExtension(fileName)}/720p.{fileExtension}" },

                new { Resolution = "native", Url = $"/processed/{Path.GetFileNameWithoutExtension(fileName)}/native.{fileExtension}" }
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
            var fileExtension = ".mp4";

            if (Directory.Exists(processedPath))
            {
                var hasNative = System.IO.File.Exists(Path.Combine(processedPath, "native" + fileExtension));

                var has720p = System.IO.File.Exists(Path.Combine(processedPath, "720p" + fileExtension));

                if (hasNative) versions.Add("native");
                if (has720p) versions.Add("720p");

                var isProcessingComplete = hasNative || has720p;

                videos.Add(new
                {
                    fileName = file,
                    originalUrl = $"/uploads/{file}",
                    processedVersions = versions,
                    isProcessing = !isProcessingComplete
                });
            }
            else
            {
                videos.Add(new
                {
                    fileName = file,
                    originalUrl = $"/uploads/{file}",
                    processedVersions = new List<string>(),
                    isProcessing = true
                });
            }
        }

        return Ok(videos);
    }
}