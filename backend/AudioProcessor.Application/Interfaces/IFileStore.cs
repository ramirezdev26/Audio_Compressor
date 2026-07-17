namespace AudioProcessor.Application.Interfaces;

public interface IFileStore
{
    Task<string> SaveAsync(Stream fileStream, string fileName);
}