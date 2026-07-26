using Microsoft.EntityFrameworkCore;
using AudioProcessor.Infrastructure.Data;
using AudioProcessor.Application.Interfaces;
using AudioProcessor.Application.Audio;
using AudioProcessor.Infrastructure.Storage;
using AudioProcessor.Infrastructure.Compression;
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
