namespace AudioProcessor.Domain.Entities;

public class AudioFile
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string CompressedUrl { get; set; } = string.Empty;
    public long CompressionTimeMs { get; set; }
}