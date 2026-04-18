namespace NotionManagerNet.Interfaces;

public interface INotionService
{
    Task<int> ExecutarAsync(CancellationToken cancellationToken = default);
}
