using System.Threading.Channels;
using FFMpegCore;

namespace VideoUploadService.Services;

public class VideoProcessingService : BackgroundService
{
    private readonly Channel<VideoProcessingJob> _channel;
    private readonly string _processedFolder;

    private readonly ILogger<VideoProcessingService> _logger;
    private readonly int _maxConcurrentProcessing;

    public VideoProcessingService(ILogger<VideoProcessingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _channel = Channel.CreateUnbounded<VideoProcessingJob>();
        _processedFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "processed");
        _maxConcurrentProcessing = Math.Max(1, Environment.ProcessorCount - 1);

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
        _logger.LogInformation("Video processing service started");
        using var semaphore = new SemaphoreSlim(_maxConcurrentProcessing);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _channel.Reader.ReadAsync(stoppingToken);
                await semaphore.WaitAsync(stoppingToken);

                _ = ProcessedVideoWithRelease(job, semaphore).ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        _logger.LogError(t.Exception, "Error processing video: {FileName}", job.FileName);
                    }
                    else if (t.IsCompleted)
                    {
                        _logger.LogInformation("Successfully processed video: {FileName}", job.FileName);
                    }
                }, TaskScheduler.Default);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Video processing service stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExecuteAsync");
            }
        }
    }

    private async Task ProcessedVideoWithRelease(VideoProcessingJob job, SemaphoreSlim semaphore)
    {
        try
        {
            await ProcessVideo(job);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task ProcessVideo(VideoProcessingJob job)
    {
        try
        {
            Console.WriteLine($"\nStarting video processing for {job.FileName}");
            var videoFolder = Path.Combine(_processedFolder, Path.GetFileNameWithoutExtension(job.FileName));
            Directory.CreateDirectory(videoFolder);

            // Copy original file
            try
            {
                Console.WriteLine($"[{job.FileName}] Copying original file...");
                var originalDestination = Path.Combine(videoFolder, "original" + Path.GetExtension(job.FileName));
                File.Copy(job.OriginalFilePath, originalDestination, true);
                Console.WriteLine($"[{job.FileName}] ✓ Original file copied");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed during original file copy: {ex.Message}");
            }

            var inputPath = job.OriginalFilePath;
            var nativeInputPath = job.OriginalFilePath;

            // Convert MKV to MP4 if needed
            if (Path.GetExtension(job.FileName).Equals(".mkv", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Console.WriteLine($"[{job.FileName}] Converting MKV to MP4...");
                    var mp4Path = Path.Combine(videoFolder, "source.mp4");
                    await FFMpegArguments
                        .FromFileInput(inputPath)
                        .OutputToFile(mp4Path, true, options => options
                            .WithCustomArgument("-c copy"))
                        .ProcessAsynchronously(true);

                    inputPath = mp4Path;
                    Console.WriteLine($"[{job.FileName}] ✓ MKV conversion completed");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed during MKV conversion: {ex.Message}");
                }
            }

            // Process native resolution version
            try
            {
                Console.WriteLine($"[{job.FileName}] Processing native resolution version...");
                var outputNative = Path.Combine(videoFolder, "native.mp4");
                await FFMpegArguments
                    .FromFileInput(nativeInputPath)
                    .OutputToFile(outputNative, true, options => options
                        .WithCustomArgument("-c copy")
                        .WithFastStart())
                    .ProcessAsynchronously(true);
                Console.WriteLine($"[{job.FileName}] ✓ Native resolution version completed");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed during native version processing: {ex.Message}");
            }
            //TODO:optimize and add back later 
            // Process 720p version
            try
            {
                Console.WriteLine($"[{job.FileName}] Processing 720p version...");
                var output720p = Path.Combine(videoFolder, "720p.mp4");
                await FFMpegArguments
                    .FromFileInput(inputPath)
                    .OutputToFile(output720p, true, options => options
                        .WithVideoFilters(filterOptions => filterOptions
                            .Scale(width: 1280, height: 720))
                        .WithConstantRateFactor(23)
                        .WithFastStart())
                    .ProcessAsynchronously(true);
                Console.WriteLine($"[{job.FileName}] ✓ 720p version completed");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed during 720p version processing: {ex.Message}");
            }




            Console.WriteLine($"[{job.FileName}] ✓ All processing completed successfully!\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{job.FileName}] ❌ Error: {ex.Message}");
            throw; // Re-throw to ensure the error is properly handled by ProcessedVideoWithRelease
        }
    }
}

public class VideoProcessingJob
{
    public string OriginalFilePath { get; set; }
    public string FileName { get; set; }
}