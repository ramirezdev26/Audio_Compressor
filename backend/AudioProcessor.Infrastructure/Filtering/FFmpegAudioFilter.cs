using AudioProcessor.Application.Interfaces;
using Xabe.FFmpeg;

namespace AudioProcessor.Infrastructure.Filtering;

public class FFmpegAudioFilter : IAudioFilter
{
    public async Task<string> ApplyNoiseReductionAsync(string inputFilePath, string outputFilePath)
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(inputFilePath);
        var audioStream = mediaInfo.AudioStreams.First();

        await FFmpeg.Conversions.New()
            .AddStream(audioStream)
            .AddParameter("-af afftdn")
            .SetOutput(outputFilePath)
            .Start();

        return outputFilePath;
    }
}
