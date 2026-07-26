using System.Diagnostics;
using AudioProcessor.Application.Interfaces;
using AudioProcessor.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AudioProcessor.Application.Audio;

public class UploadAudioService
{
    private readonly IFileStore _fileStore;
    private readonly IAudioRepository _audioRepository;
    private readonly IAudioCompressor _audioCompressor;
    private readonly ILogger<UploadAudioService> _logger;

    public UploadAudioService(
        IFileStore fileStore,
        IAudioRepository audioRepository,
        IAudioCompressor audioCompressor,
        ILogger<UploadAudioService> logger)
    {
        _fileStore = fileStore;
        _audioRepository = audioRepository;
        _audioCompressor = audioCompressor;
        _logger = logger;
    }

    public async Task<AudioFile> UploadAsync(Stream fileStream, string originalFileName)
    {
        var id = Guid.NewGuid();
        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{id}{extension}";
        var compressedFileName = $"{id}_compressed.aac";

        var tempInput = Path.Combine(Path.GetTempPath(), storedFileName);
        var tempOutput = Path.Combine(Path.GetTempPath(), compressedFileName);

        try
        {
            using (var tempStream = new FileStream(tempInput, FileMode.Create))
                await fileStream.CopyToAsync(tempStream);

            string url;
            using (var s = File.OpenRead(tempInput))
                url = await _fileStore.SaveAsync(s, storedFileName);

            var sw = Stopwatch.StartNew();
            await _audioCompressor.CompressToAacAsync(tempInput, tempOutput);
            sw.Stop();
            var compressionTimeMs = sw.ElapsedMilliseconds;

            string compressedUrl;
            using (var s = File.OpenRead(tempOutput))
                compressedUrl = await _fileStore.SaveAsync(s, compressedFileName);

            _logger.LogInformation("Audio {Id} compressed in {ElapsedMs} ms", id, compressionTimeMs);

            var audioFile = new AudioFile
            {
                Id = id,
                Url = url,
                CompressedUrl = compressedUrl,
                CompressionTimeMs = compressionTimeMs
            };
            await _audioRepository.AddAsync(audioFile);

            return audioFile;
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
            if (File.Exists(tempOutput)) File.Delete(tempOutput);
        }
    }
}
