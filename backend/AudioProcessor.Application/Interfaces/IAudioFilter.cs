namespace AudioProcessor.Application.Interfaces;

public interface IAudioFilter
{
    Task<string> ApplyNoiseReductionAsync(string inputFilePath, string outputFilePath);
}
