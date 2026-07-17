using AudioProcessor.Application.Interfaces;

namespace AudioProcessor.Infrastructure.Storage;

public class LocalFileStore : IFileStore
{
    private readonly string _basePath = Path.Combine(Directory.GetCurrentDirectory(), "FileStore");

    public async Task<string> SaveAsync(Stream fileStream, string fileName)
    {
        Directory.CreateDirectory(_basePath);
        var path = Path.Combine(_basePath, fileName);

        using var output = new FileStream(path, FileMode.Create);
        await fileStream.CopyToAsync(output);

        return $"/files/{fileName}";
    }
}