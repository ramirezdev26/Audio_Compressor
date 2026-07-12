using Microsoft.EntityFrameworkCore;
using AudioProcessor.Infrastructure.Data;
using AudioProcessor.Application.Interfaces;
using AudioProcessor.Application.Audio;
using AudioProcessor.Infrastructure.Storage;
using AudioProcessor.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=audio.db"));
builder.Services.AddScoped<IFileStore, LocalFileStore>();
builder.Services.AddScoped<IAudioRepository, AudioRepository>();
builder.Services.AddScoped<UploadAudioService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
