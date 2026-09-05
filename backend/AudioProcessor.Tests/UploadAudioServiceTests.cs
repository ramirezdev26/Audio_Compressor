using AudioProcessor.Application.Audio;
using AudioProcessor.Application.Interfaces;
using AudioProcessor.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace AudioProcessor.Tests;

public class UploadAudioServiceTests
{
    private const int MaxSummaryLength = 50;

    private sealed class FakeFileStore : IFileStore
    {
        public Task<string> SaveAsync(Stream fileStream, string fileName)
            => Task.FromResult($"/files/{fileName}");
    }

    private sealed class FakeAudioRepository : IAudioRepository
    {
        public AudioFile? Saved { get; private set; }
        public Task AddAsync(AudioFile audioFile) { Saved = audioFile; return Task.CompletedTask; }
        public Task<List<AudioFile>> GetAllAsync() => Task.FromResult(Saved is null ? new List<AudioFile>() : new List<AudioFile> { Saved });
    }

    private sealed class FakeAudioCompressor : IAudioCompressor
    {
        public Task<string> CompressToAacAsync(string inputFilePath, string outputFilePath)
        {
            File.WriteAllBytes(outputFilePath, new byte[] { 0xFF, 0xF1 });
            return Task.FromResult(outputFilePath);
        }
    }

    private sealed class FakeAudioFilter : IAudioFilter
    {
        public Task<string> ApplyNoiseReductionAsync(string inputFilePath, string outputFilePath)
        {
            File.WriteAllBytes(outputFilePath, new byte[] { 0xFF, 0xF1 });
            return Task.FromResult(outputFilePath);
        }
    }

    private sealed class FakeAudioTranscriber : IAudioTranscriber
    {
        private readonly string _transcript;
        public FakeAudioTranscriber(string transcript) => _transcript = transcript;
        public Task<string> TranscribeAsync(string audioFilePath) => Task.FromResult(_transcript);
    }

    private sealed class FakeTextSummarizer : ITextSummarizer
    {
        public Task<string> SummarizeAsync(string text, int maxLength)
        {
            var summary = text.Length > maxLength ? text[..maxLength] : text;
            return Task.FromResult(summary);
        }
    }

    private static UploadAudioService CreateService(
        FakeAudioRepository repo,
        string transcript = "Esta es una transcripción de prueba con contenido suficiente.")
    {
        return new UploadAudioService(
            new FakeFileStore(),
            repo,
            new FakeAudioCompressor(),
            new FakeAudioTranscriber(transcript),
            new FakeTextSummarizer(),
            new FakeAudioFilter(),
            NullLogger<UploadAudioService>.Instance);
    }

    [Fact]
    public async Task UploadAsync_ReturnsNonEmptyCompressedUrl()
    {
        var repo = new FakeAudioRepository();
        var service = CreateService(repo);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var result = await service.UploadAsync(stream, "test.mp3");

        Assert.False(string.IsNullOrEmpty(result.CompressedUrl));
    }

    [Fact]
    public async Task UploadAsync_SavedEntityHasNonEmptyCompressedUrl()
    {
        var repo = new FakeAudioRepository();
        var service = CreateService(repo);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        await service.UploadAsync(stream, "test.wav");

        Assert.NotNull(repo.Saved);
        Assert.False(string.IsNullOrEmpty(repo.Saved!.CompressedUrl));
    }

    [Fact]
    public async Task UploadAsync_WithLongTranscript_SummaryIsAtMost50Chars()
    {
        var repo = new FakeAudioRepository();
        var longTranscript = new string('a', 200);
        var service = CreateService(repo, longTranscript);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var result = await service.UploadAsync(stream, "test.mp3");

        Assert.True(result.Summary.Length <= MaxSummaryLength);
    }

    [Fact]
    public async Task UploadAsync_SavedEntityHasNonEmptyFilteredUrl()
    {
        var repo = new FakeAudioRepository();
        var service = CreateService(repo);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        await service.UploadAsync(stream, "test.wav");

        Assert.NotNull(repo.Saved);
        Assert.False(string.IsNullOrEmpty(repo.Saved!.FilteredUrl));
    }

    [Fact]
    public async Task UploadAsync_WithTranscript_SummaryIsNotEmpty()
    {
        var repo = new FakeAudioRepository();
        var service = CreateService(repo, "Transcripción no vacía para el audio de prueba.");

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var result = await service.UploadAsync(stream, "test.mp3");

        Assert.False(string.IsNullOrEmpty(result.Summary));
    }
}
