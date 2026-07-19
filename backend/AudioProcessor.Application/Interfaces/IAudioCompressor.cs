namespace AudioProcessor.Application.Interfaces;

public interface IAudioCompressor
{
    Task<string> CompressToAacAsync(string inputFilePath, string outputFilePath);
}
