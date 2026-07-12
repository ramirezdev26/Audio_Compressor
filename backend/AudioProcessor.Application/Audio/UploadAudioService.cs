using AudioProcessor.Application.Interfaces;
using AudioProcessor.Domain.Entities;

namespace AudioProcessor.Application.Audio;

public class UploadAudioService
{
    private readonly IFileStore _fileStore;
    private readonly IAudioRepository _audioRepository;

    public UploadAudioService(IFileStore fileStore, IAudioRepository audioRepository)
    {
        _fileStore = fileStore;
        _audioRepository = audioRepository;
    }

    public async Task<AudioFile> UploadAsync(Stream fileStream, string originalFileName)
    {
        var id = Guid.NewGuid();
        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{id}{extension}";

        var url = await _fileStore.SaveAsync(fileStream, storedFileName);

        var audioFile = new AudioFile { Id = id, Url = url };
        await _audioRepository.AddAsync(audioFile);

        return audioFile;
    }
}