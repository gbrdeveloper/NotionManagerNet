namespace NotionManagerNet.Services;

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotionManagerNet.Configurations;
using NotionManagerNet.Interfaces;
using NotionManagerNet.Mappers;
using NotionManagerNet.Models;

public sealed class NotionService(
    IHttpClientFactory httpClientFactory,
    NotionSettings settings,
    ILogger<NotionService> logger,
    CardClassifier classifier,
    ITelegramService telegramService
) : INotionService
{
    private const string BaseUrl = "https://api.notion.com/v1";
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly NotionSettings _settings = settings;
    private readonly ILogger<NotionService> _logger = logger;
    private readonly CardClassifier _classifier = classifier;
    private readonly ITelegramService _telegramService = telegramService;

    public async Task<int> ExecutarAsync(CancellationToken cancellationToken = default)
    {
        var todosCards = await BuscarCardsAsync(cancellationToken);

        var cards = todosCards
            .Where(c => c.Prazo is not null && !CardClassifier.IgnoredStatuses.Contains(c.Status))
            .ToList();

        _logger.LogInformation(
            "Total: {Total} | Elegíveis para processamento: {Elegiveis}",
            todosCards.Count,
            cards.Count
        );

        var processados = 0;
        var erros = 0;
        var atualizacoes = new List<(TaskCard Card, string StatusAnterior, string NovoStatus)>();

        foreach (var card in cards)
        {
            try
            {
                var statusAnterior = card.Status;
                var novoStatus = _classifier.Classify(card);
                await ProcessarCardAsync(card, cancellationToken);

                if (novoStatus != statusAnterior)
                    atualizacoes.Add((card, statusAnterior, novoStatus));

                processados++;
            }
            catch (Exception ex)
            {
                erros++;
                _logger.LogError(ex, "Erro ao processar card {CardId}", card.Id);
            }
        }

        await _telegramService.EnviarResumoAsync(
            atualizacoes,
            processados,
            erros,
            cancellationToken
        );

        _logger.LogInformation(
            "Concluído | Processados: {Processados} | Atualizados: {Atualizados} | Erros: {Erros}",
            processados,
            atualizacoes.Count,
            erros
        );

        return atualizacoes.Count;
    }

    private async Task ProcessarCardAsync(
        TaskCard card,
        CancellationToken cancellationToken = default
    )
    {
        var novoStatus = _classifier.Classify(card);

        if (novoStatus == card.Status)
        {
            _logger.LogDebug(
                "Card {CardId} sem alteração de status '{Status}'",
                card.Id,
                card.Status
            );
            return;
        }

        _logger.LogInformation(
            "Card {CardId} '{StatusAtual}' → '{NovoStatus}'",
            card.Id,
            card.Status,
            novoStatus
        );

        await AtualizarStatusAsync(card.Id, novoStatus, cancellationToken);
    }

    public async Task<IReadOnlyList<TaskCard>> BuscarCardsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var client = _httpClientFactory.CreateClient("Notion");
        var cards = new List<TaskCard>();
        string? cursor = null;
        bool temMaisPaginas;

        do
        {
            var body = new Dictionary<string, object> { ["page_size"] = 100 };

            if (cursor is not null)
                body["start_cursor"] = cursor;

            var response = await client.PostAsJsonAsync(
                $"{BaseUrl}/databases/{_settings.DatabaseId}/query",
                body,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Erro ao buscar cards: {StatusCode} - {Erro}",
                    response.StatusCode,
                    erro
                );
                throw new HttpRequestException($"Erro ao buscar cards: {response.StatusCode}");
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(
                cancellationToken: cancellationToken
            );

            foreach (var result in json.GetProperty("results").EnumerateArray())
            {
                var card = NotionCardMapper.MapCard(result, _logger);
                if (card is not null)
                    cards.Add(card);
            }

            temMaisPaginas = json.GetProperty("has_more").GetBoolean();
            cursor = temMaisPaginas ? json.GetProperty("next_cursor").GetString() : null;
        } while (temMaisPaginas);

        _logger.LogInformation("Total de cards encontrados: {Total}", cards.Count);
        return cards;
    }

    public async Task AtualizarStatusAsync(
        string cardId,
        string novoStatus,
        CancellationToken cancellationToken = default
    )
    {
        var client = _httpClientFactory.CreateClient("Notion");

        var body = new Dictionary<string, object>
        {
            ["properties"] = new Dictionary<string, object>
            {
                ["Status"] = new Dictionary<string, object>
                {
                    ["select"] = new Dictionary<string, object> { ["name"] = novoStatus },
                },
            },
        };

        var response = await client.PatchAsJsonAsync(
            $"{BaseUrl}/pages/{cardId}",
            body,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Erro ao atualizar card {CardId}: {StatusCode} - {Erro}",
                cardId,
                response.StatusCode,
                erro
            );
            throw new HttpRequestException(
                $"Erro ao atualizar card {cardId}: {response.StatusCode}"
            );
        }

        _logger.LogInformation("Card {CardId} atualizado para '{Status}'", cardId, novoStatus);
    }
}
