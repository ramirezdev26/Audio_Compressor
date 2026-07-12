using AudioProcessor.Application.Interfaces;
using AudioProcessor.Domain.Entities;

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
}