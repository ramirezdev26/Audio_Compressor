using Microsoft.EntityFrameworkCore;
using AudioProcessor.Infrastructure.Data;
using AudioProcessor.Application.Interfaces;
using AudioProcessor.Application.Audio;
using AudioProcessor.Infrastructure.Storage;
using AudioProcessor.Infrastructure.Compression;
using AudioProcessor.Infrastructure.Transcription;
using AudioProcessor.Infrastructure.Summarization;
using Whisper.net.Ggml;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=audio.db"));
builder.Services.AddScoped<IFileStore, LocalFileStore>();
builder.Services.AddScoped<IAudioRepository, AudioRepository>();
builder.Services.AddScoped<IAudioCompressor, FFmpegAudioCompressor>();

var whisperModelPath = Path.Combine(Directory.GetCurrentDirectory(), "whisper-models", "ggml-tiny.bin");
builder.Services.AddSingleton<IAudioTranscriber>(new WhisperAudioTranscriber(whisperModelPath));

builder.Services.AddHttpClient<ITextSummarizer, OllamaTextSummarizer>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["Ollama:BaseUrl"] ?? "http://localhost:11434";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped<UploadAudioService>();

var app = builder.Build();

// Set up FFmpeg: download to local path if not present, else use system installation
var ffmpegLocalPath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg-bin");
Directory.CreateDirectory(ffmpegLocalPath);

var localBinary = Path.Combine(ffmpegLocalPath, "ffmpeg");
if (File.Exists(localBinary))
{
    FFmpeg.SetExecutablesPath(ffmpegLocalPath);
}
else
{
    // Try to download; if it fails (e.g. dependency mismatch), fall back to system PATH
    try
    {
        await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegLocalPath);
        FFmpeg.SetExecutablesPath(ffmpegLocalPath);
    }
    catch
    {
        // FFmpeg is available system-wide (e.g. installed via apt)
        FFmpeg.SetExecutablesPath("/usr/bin");
    }
}

// Set up Whisper: download the tiny model to local path if not present, else reuse it
Directory.CreateDirectory(Path.GetDirectoryName(whisperModelPath)!);
if (!File.Exists(whisperModelPath))
{
    using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(
        GgmlType.Tiny, QuantizationType.NoQuantization, CancellationToken.None);
    using var fileWriter = File.OpenWrite(whisperModelPath);
    await modelStream.CopyToAsync(fileWriter);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
