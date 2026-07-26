using AudioProcessor.Application.Audio;
using AudioProcessor.Application.Interfaces;
using AudioProcessor.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace AudioProcessor.Tests;

public class UploadAudioServiceTests
{
    private sealed class FakeFileStore : IFileStore
    {
        public Task<string> SaveAsync(Stream fileStream, string fileName)
            => Task.FromResult($"/files/{fileName}");
    }

    private sealed class FakeAudioRepository : IAudioRepository
    {
        public AudioFile? Saved { get; private set; }
        public Task AddAsync(AudioFile audioFile) { Saved = audioFile; return Task.CompletedTask; }
    }

    private sealed class FakeAudioCompressor : IAudioCompressor
    {
        public Task<string> CompressToAacAsync(string inputFilePath, string outputFilePath)
        {
            File.WriteAllBytes(outputFilePath, new byte[] { 0xFF, 0xF1 });
            return Task.FromResult(outputFilePath);
        }
    }

    [Fact]
    public async Task UploadAsync_ReturnsNonEmptyCompressedUrl()
    {
        var repo = new FakeAudioRepository();
        var service = new UploadAudioService(
            new FakeFileStore(),
            repo,
            new FakeAudioCompressor(),
            NullLogger<UploadAudioService>.Instance);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var result = await service.UploadAsync(stream, "test.mp3");

        Assert.False(string.IsNullOrEmpty(result.CompressedUrl));
    }

    [Fact]
    public async Task UploadAsync_SavedEntityHasNonEmptyCompressedUrl()
    {
        var repo = new FakeAudioRepository();
        var service = new UploadAudioService(
            new FakeFileStore(),
            repo,
            new FakeAudioCompressor(),
            NullLogger<UploadAudioService>.Instance);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        await service.UploadAsync(stream, "test.wav");

        Assert.NotNull(repo.Saved);
        Assert.False(string.IsNullOrEmpty(repo.Saved!.CompressedUrl));
    }
}
