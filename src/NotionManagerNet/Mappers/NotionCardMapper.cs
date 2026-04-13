namespace NotionManagerNet.Mappers;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotionManagerNet.Models;

public static class NotionCardMapper
{
    public static TaskCard? MapCard(JsonElement result, ILogger? logger = null)
    {
        try
        {
            var id = result.GetProperty("id").GetString()!;
            var properties = result.GetProperty("properties");

            var nome =
                properties
                    .GetProperty("Name")
                    .GetProperty("title")
                    .EnumerateArray()
                    .FirstOrDefault()
                    .GetProperty("plain_text")
                    .GetString()
                ?? string.Empty;

            var status =
                properties
                    .GetProperty("Status")
                    .GetProperty("select")
                    .GetProperty("name")
                    .GetString()
                ?? string.Empty;

            string? prioridade = null;
            if (
                properties.TryGetProperty("Prioridade", out var prioridadeProperty)
                && prioridadeProperty.GetProperty("select").ValueKind != JsonValueKind.Null
            )
            {
                prioridade = prioridadeProperty
                    .GetProperty("select")
                    .GetProperty("name")
                    .GetString();
            }

            DateTime? prazo = null;
            if (
                properties.TryGetProperty("Prazo", out var prazoProperty)
                && prazoProperty.GetProperty("date").ValueKind != JsonValueKind.Null
            )
            {
                var startStr = prazoProperty.GetProperty("date").GetProperty("start").GetString();

                if (DateTime.TryParse(startStr, out var parsedDate))
                    prazo = parsedDate;
            }

            decimal? valor = null;
            if (
                properties.TryGetProperty("Valor", out var valorProperty)
                && valorProperty.GetProperty("number").ValueKind != JsonValueKind.Null
            )
            {
                valor = valorProperty.GetProperty("number").GetDecimal();
            }

            return new TaskCard(id, nome, status, prazo, prioridade, valor);
        }
        catch (Exception ex)
        {
            string cardId;
            try
            {
                cardId = result.GetProperty("id").GetString() ?? string.Empty;
            }
            catch
            {
                cardId = string.Empty;
            }

            logger?.LogError(ex, "Erro ao mapear card: {CardId}", cardId);
            return null;
        }
    }
}
