namespace NotionManagerNet.Models;

public sealed class TaskCard
{
    public string Id { get; init; }
    public string Nome { get; init; }
    public string Status { get; init; }
    public DateTime? Prazo { get; init; }
    public string? Prioridade { get; init; }
    public decimal? Valor { get; init; }

    public TaskCard(
        string id,
        string nome,
        string status,
        DateTime? prazo,
        string? prioridade = null,
        decimal? valor = null
    )
    {
        Id = !string.IsNullOrWhiteSpace(id)
            ? id
            : throw new ArgumentException("Id é obrigatório.", nameof(id));

        Nome = !string.IsNullOrWhiteSpace(nome)
            ? nome
            : throw new ArgumentException("Nome é obrigatório.", nameof(nome));

        Status = !string.IsNullOrWhiteSpace(status)
            ? status
            : throw new ArgumentException("Status é obrigatório.", nameof(status));

        Prazo = prazo;
        Prioridade = prioridade;
        Valor = valor;
    }
}
