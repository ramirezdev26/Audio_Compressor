using System.Text;
using AudioProcessor.Application.Interfaces;
using Whisper.net;
using Xabe.FFmpeg;

namespace AudioProcessor.Infrastructure.Transcription;

public class WhisperAudioTranscriber : IAudioTranscriber
{
    private readonly string _modelPath;

    public WhisperAudioTranscriber(string modelPath)
    {
        _modelPath = modelPath;
    }

    public async Task<string> TranscribeAsync(string audioFilePath)
    {
        var wavPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");

        try
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(audioFilePath);
            var audioStream = mediaInfo.AudioStreams.First()
                .SetCodec(AudioCodec.pcm_s16le)
                .SetSampleRate(16000)
                .SetChannels(1);

            await FFmpeg.Conversions.New()
                .AddStream(audioStream)
                .SetOutput(wavPath)
                .Start();

            using var whisperFactory = WhisperFactory.FromPath(_modelPath);
            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("auto")
                .Build();

            using var wavStream = File.OpenRead(wavPath);

            var transcript = new StringBuilder();
            await foreach (var segment in processor.ProcessAsync(wavStream))
            {
                transcript.Append(segment.Text);
            }

            return transcript.ToString().Trim();
        }
        finally
        {
            if (File.Exists(wavPath)) File.Delete(wavPath);
        }
    }
}
