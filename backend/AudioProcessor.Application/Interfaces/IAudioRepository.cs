using AudioProcessor.Domain.Entities;

namespace AudioProcessor.Application.Interfaces;

public interface IAudioRepository
{
    Task AddAsync(AudioFile audioFile);
}