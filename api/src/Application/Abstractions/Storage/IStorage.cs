using SharedKernel;

namespace Application.Abstractions.Storage;

public interface IStorage
{
    Task<Result> SaveAsync(string path, Stream content, CancellationToken cancellationToken = default);

    Task<Result<string>> DownloadAsync(string path, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(string path, CancellationToken cancellationToken = default);

    Task<Result<bool>> ExistsAsync(string path, CancellationToken cancellationToken = default);
}
