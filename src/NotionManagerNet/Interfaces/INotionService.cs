namespace NotionManagerNet.Interfaces;

public interface INotionService
{
    Task ExecutarAsync(CancellationToken cancellationToken = default);
}
