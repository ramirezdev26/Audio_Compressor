using Microsoft.AspNetCore.Mvc;
using AudioProcessor.Application.Audio;

namespace AudioProcessor.Api.Controllers;

[ApiController]
[Route("api/audio")]
public class AudioController : ControllerBase
{
    private readonly UploadAudioService _uploadAudioService;

    public AudioController(UploadAudioService uploadAudioService)
    {
        _uploadAudioService = uploadAudioService;
    }

    [HttpPost]
    public async Task<IActionResult> UploadAudio(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file was provided.");

        using var stream = file.OpenReadStream();
        var result = await _uploadAudioService.UploadAsync(stream, file.FileName);

        return Ok(new
        {
            result.Id,
            result.Url,
            result.CompressedUrl,
            result.CompressionTimeMs
        });
    }
}
