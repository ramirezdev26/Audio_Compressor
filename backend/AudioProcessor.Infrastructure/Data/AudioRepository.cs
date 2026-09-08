using AudioProcessor.Application.Interfaces;
using AudioProcessor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AudioProcessor.Infrastructure.Data;

public class AudioRepository : IAudioRepository
{
    private readonly AppDbContext _db;

    public AudioRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(AudioFile audioFile)
    {
        _db.AudioFiles.Add(audioFile);
        await _db.SaveChangesAsync();
    }

    public async Task<List<AudioFile>> GetAllAsync()
    {
        return await _db.AudioFiles
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync();
    }
}