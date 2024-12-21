using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VideoUploadService.Models;
using VideoUploadService.Services;

namespace VideoUploadService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoController : ControllerBase
{
    private readonly string _uploadFolder;
    private readonly string _metadataFile;
    private readonly VideoProcessingService _processingService;

    public VideoController(VideoProcessingService processingService)
    {
        _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _metadataFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "metadata.json");
        _processingService = processingService;

        if (!Directory.Exists(_uploadFolder))
            Directory.CreateDirectory(_uploadFolder);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadVideo([FromForm] IFormFile file, [FromForm] string name, [FromForm] string genre)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        // Create a safe filename using the original name
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{originalFileName}_{timestamp}{extension}";

        // Remove any invalid characters from filename
        fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));

        var filePath = Path.Combine(_uploadFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Save metadata
        var metadata = new VideoMetadata
        {
            FileName = fileName,
            Name = name,
            Genre = genre,
            FilePath = filePath,
            UploadDate = DateTime.UtcNow
        };
        await SaveMetadata(metadata);

        // Queue for processing
        await _processingService.QueueVideoForProcessing(new VideoProcessingJob
        {
            OriginalFilePath = filePath,
            FileName = fileName
        });

        return Ok(new { FileName = fileName });
    }

    private async Task SaveMetadata(VideoMetadata metadata)
    {
        var existingMetadata = await LoadAllMetadata();
        existingMetadata.Add(metadata);
        await System.IO.File.WriteAllTextAsync(_metadataFile, JsonSerializer.Serialize(existingMetadata));
    }

    private async Task<List<VideoMetadata>> LoadAllMetadata()
    {
        if (!System.IO.File.Exists(_metadataFile))
            return new List<VideoMetadata>();

        var json = await System.IO.File.ReadAllTextAsync(_metadataFile);
        return JsonSerializer.Deserialize<List<VideoMetadata>>(json) ?? new List<VideoMetadata>();
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
    public async Task<IActionResult> ListVideos()
    {
        var metadata = await LoadAllMetadata();
        var processedFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "processed");
        var videos = new List<object>();

        foreach (var meta in metadata)
        {
            var processedPath = Path.Combine(processedFolder, Path.GetFileNameWithoutExtension(meta.FileName));
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
                    fileName = meta.FileName,
                    name = meta.Name,
                    genre = meta.Genre,
                    originalUrl = $"/uploads/{meta.FileName}",
                    processedVersions = versions,
                    isProcessing = !isProcessingComplete
                });
            }
            else
            {
                videos.Add(new
                {
                    fileName = meta.FileName,
                    name = meta.Name,
                    genre = meta.Genre,
                    originalUrl = $"/uploads/{meta.FileName}",
                    processedVersions = new List<string>(),
                    isProcessing = true
                });
            }
        }

        return Ok(videos);
    }
}