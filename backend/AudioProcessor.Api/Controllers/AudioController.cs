using Microsoft.AspNetCore.Mvc;
using AudioProcessor.Application.Audio;
using AudioProcessor.Application.Interfaces;

namespace AudioProcessor.Api.Controllers;

[ApiController]
[Route("api/audio")]
public class AudioController : ControllerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAudioRepository _audioRepository;
    private readonly SemaphoreSlim _processingSemaphore;
    private readonly ILogger<AudioController> _logger;

    public AudioController(
        IServiceScopeFactory scopeFactory,
        IAudioRepository audioRepository,
        SemaphoreSlim processingSemaphore,
        ILogger<AudioController> logger)
    {
        _scopeFactory = scopeFactory;
        _audioRepository = audioRepository;
        _processingSemaphore = processingSemaphore;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAudio()
    {
        var audioFiles = await _audioRepository.GetAllAsync();

        var result = audioFiles.Select(a => new
        {
            id = a.Id,
            url = a.Url,
            compressedUrl = a.CompressedUrl,
            filteredUrl = a.FilteredUrl,
            uploadedAt = a.UploadedAt
        });

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> UploadAudio(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file was provided.");

        var id = Guid.NewGuid();
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"{id}_{file.FileName}");

        using (var tempStream = new FileStream(tempFilePath, FileMode.Create))
            await file.CopyToAsync(tempStream);

        _ = Task.Run(() => ProcessInBackgroundAsync(id, tempFilePath, file.FileName));

        return Accepted(new
        {
            id,
            message = "Procesando en segundo plano"
        });
    }

    private async Task ProcessInBackgroundAsync(Guid id, string tempFilePath, string originalFileName)
    {
        _logger.LogInformation("Audio {Id} waiting for a processing slot", id);
        await _processingSemaphore.WaitAsync();
        _logger.LogInformation("Audio {Id} acquired a processing slot, starting background processing", id);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var uploadAudioService = scope.ServiceProvider.GetRequiredService<UploadAudioService>();

            using (var stream = System.IO.File.OpenRead(tempFilePath))
                await uploadAudioService.UploadAsync(stream, originalFileName);

            _logger.LogInformation("Background processing finished for audio {Id}", id);
        }
        finally
        {
            _processingSemaphore.Release();
            if (System.IO.File.Exists(tempFilePath)) System.IO.File.Delete(tempFilePath);
        }
    }
}
