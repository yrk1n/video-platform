// Services/VideoProcessingService.cs
using System.Linq.Expressions;
using System.Threading.Channels;
using FFMpegCore;

namespace VideoUploadService.Services;

public class VideoProcessingService : BackgroundService
{
    private readonly Channel<VideoProcessingJob> _channel;
    private readonly string _processedFolder;

    public VideoProcessingService()
    {
        _channel = Channel.CreateUnbounded<VideoProcessingJob>();
        _processedFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "processed");

        if (!Directory.Exists(_processedFolder))
            Directory.CreateDirectory(_processedFolder);

        GlobalFFOptions.Configure(new FFOptions { BinaryFolder = Path.Combine("C:", "ProgramData", "chocolatey", "lib", "ffmpeg", "tools", "ffmpeg", "bin") });
    }

    public async Task QueueVideoForProcessing(VideoProcessingJob job)
    {
        await _channel.Writer.WriteAsync(job);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var job = await _channel.Reader.ReadAsync(stoppingToken);
            await ProcessVideo(job);
        }
    }

    private async Task ProcessVideo(VideoProcessingJob job)
    {
        try
        {
            var videoFolder = Path.Combine(_processedFolder, Path.GetFileNameWithoutExtension(job.FileName));
            Directory.CreateDirectory(videoFolder);

            // Process 720p version
            var output720p = Path.Combine(videoFolder, "720p.mp4");
            await FFMpegArguments
                .FromFileInput(job.OriginalFilePath)
                .OutputToFile(output720p, true, options => options
                    .WithVideoFilters(filterOptions => filterOptions
                        .Scale(width: 1280, height: 720))
                    .WithConstantRateFactor(23)
                    .WithFastStart())
                .ProcessAsynchronously();

            // Process 480p version
            var output480p = Path.Combine(videoFolder, "480p.mp4");
            await FFMpegArguments
                .FromFileInput(job.OriginalFilePath)
                .OutputToFile(output480p, true, options => options
                    .WithVideoFilters(filterOptions => filterOptions
                        .Scale(width: 854, height: 480))
                    .WithConstantRateFactor(23)
                        .WithFastStart())
                        .ProcessAsynchronously();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing video: {ex.Message}");
        }
    }
}

public class VideoProcessingJob
{
    public string OriginalFilePath { get; set; }
    public string FileName { get; set; }
}