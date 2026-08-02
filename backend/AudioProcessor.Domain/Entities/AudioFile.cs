namespace AudioProcessor.Domain.Entities;

public class AudioFile
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string CompressedUrl { get; set; } = string.Empty;
    public long CompressionTimeMs { get; set; }
    public string Transcript { get; set; } = string.Empty;
    public long TranscriptionTimeMs { get; set; }
    public string Summary { get; set; } = string.Empty;
    public long SummaryTimeMs { get; set; }
}