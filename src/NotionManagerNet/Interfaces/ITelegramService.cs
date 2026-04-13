namespace NotionManagerNet.Interfaces;

using NotionManagerNet.Models;

public interface ITelegramService
{
    Task EnviarResumoAsync(
        IReadOnlyList<(TaskCard Card, string StatusAnterior, string NovoStatus)> atualizacoes,
        int processados,
        int erros,
        CancellationToken cancellationToken = default
    );
}
