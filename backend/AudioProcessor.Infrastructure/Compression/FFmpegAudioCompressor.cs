using AudioProcessor.Application.Interfaces;
using Xabe.FFmpeg;

namespace AudioProcessor.Infrastructure.Compression;

public class FFmpegAudioCompressor : IAudioCompressor
{
    public async Task<string> CompressToAacAsync(string inputFilePath, string outputFilePath)
    {
        var mediaInfo = await FFmpeg.GetMediaInfo(inputFilePath);
        var audioStream = mediaInfo.AudioStreams.First().SetCodec(AudioCodec.aac);

        await FFmpeg.Conversions.New()
            .AddStream(audioStream)
            .SetOutput(outputFilePath)
            .Start();

        return outputFilePath;
    }
}
