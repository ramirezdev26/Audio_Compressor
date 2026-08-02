namespace AudioProcessor.Application.Interfaces;

public interface IAudioTranscriber
{
    Task<string> TranscribeAsync(string audioFilePath);
}
