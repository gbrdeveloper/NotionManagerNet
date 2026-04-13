namespace NotionManagerNet.Services;

using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using NotionManagerNet.Interfaces;
using NotionManagerNet.Models;

public sealed class TelegramService : ITelegramService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TelegramService> _logger;
    private readonly NotionManagerNet.Configurations.TelegramSettings _settings;

    public TelegramService(
        IHttpClientFactory httpClientFactory,
        ILogger<TelegramService> logger,
        NotionManagerNet.Configurations.TelegramSettings settings
    )
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _settings = settings;
    }

    private const string BaseUrl = "https://api.telegram.org";

    private static readonly TimeZoneInfo BrasiliaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
        "E. South America Standard Time"
    );

    public async Task EnviarResumoAsync(
        IReadOnlyList<(TaskCard Card, string StatusAnterior, string NovoStatus)> atualizacoes,
        int processados,
        int erros,
        CancellationToken cancellationToken = default
    )
    {
        var agora = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BrasiliaTimeZone);
        var mensagem = MontarMensagem(atualizacoes, processados, erros, agora);

        var client = _httpClientFactory.CreateClient("Telegram");

        var body = new Dictionary<string, string>
        {
            ["chat_id"] = _settings.ChatId,
            ["text"] = mensagem,
            ["parse_mode"] = "HTML",
        };

        var response = await client.PostAsJsonAsync(
            $"{BaseUrl}/bot{_settings.BotToken}/sendMessage",
            body,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Erro ao enviar mensagem no Telegram: {Erro}", erro);
            return;
        }

        _logger.LogInformation("Mensagem enviada no Telegram com sucesso!");
    }

    internal static string MontarMensagem(
        IReadOnlyList<(TaskCard Card, string StatusAnterior, string NovoStatus)> atualizacoes,
        int processados,
        int erros,
        DateTime agora
    )
    {
        var sb = new StringBuilder();

        sb.AppendLine($"📋 <b>Notion Manager</b>");
        sb.AppendLine($"<code>{agora:dd/MM/yyyy} às {agora:HH:mm}</code>");
        sb.AppendLine();

        if (atualizacoes.Count == 0)
        {
            sb.AppendLine("Nenhum card precisou ser atualizado.");
        }
        else
        {
            var plural = atualizacoes.Count > 1;
            sb.AppendLine(
                $"<b>{atualizacoes.Count} card{(plural ? "s" : "")} atualizado{(plural ? "s" : "")}</b>"
            );

            foreach (var (card, statusAnterior, novoStatus) in atualizacoes)
            {
                sb.AppendLine();
                sb.AppendLine($"▸ <b>{WebUtility.HtmlEncode(card.Nome)}</b>");

                if (card.Prazo.HasValue)
                {
                    var prazo = TimeZoneInfo.ConvertTimeFromUtc(
                        card.Prazo.Value.ToUniversalTime(),
                        BrasiliaTimeZone
                    );
                    var prazoStr =
                        prazo.TimeOfDay == TimeSpan.Zero
                            ? prazo.ToString("dd/MM/yyyy")
                            : prazo.ToString("dd/MM/yyyy HH:mm");
                    sb.AppendLine($"   Prazo: <code>{prazoStr}</code>");
                }

                if (!string.IsNullOrWhiteSpace(card.Prioridade))
                    sb.AppendLine($"   Prioridade: <code>{card.Prioridade}</code>");

                if (card.Valor.HasValue)
                    sb.AppendLine($"   Valor: <code>{card.Valor.Value:C2}</code>");

                sb.AppendLine($"   <i>{statusAnterior}</i> → <b>{novoStatus}</b>");
            }
        }

        sb.AppendLine();
        sb.AppendLine("─────────────────────");

        var rodapeParts = new List<string>
        {
            $"{processados} processado{(processados != 1 ? "s" : "")}",
            $"{atualizacoes.Count} atualizado{(atualizacoes.Count != 1 ? "s" : "")}",
        };
        if (erros > 0)
            rodapeParts.Add($"⚠️ {erros} erro{(erros != 1 ? "s" : "")}");

        sb.Append(string.Join(" · ", rodapeParts));

        return sb.ToString();
    }
}
