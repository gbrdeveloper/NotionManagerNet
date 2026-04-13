namespace NotionManagerNet.Tests;

using System.Text.Json;
using NotionManagerNet.Mappers;

public sealed class NotionCardMapperTests
{
    [Fact]
    public void MapCard_JsonCompletoValido_RetornaTaskCard()
    {
        var json = """
            {
                "id": "abc-123",
                "properties": {
                    "Name": { "title": [{ "plain_text": "Test Card" }] },
                    "Status": { "select": { "name": "Esta semana" } },
                    "Prazo": { "date": { "start": "2026-04-18" } },
                    "Prioridade": { "select": { "name": "Alta" } },
                    "Valor": { "number": 100.50 }
                }
            }
            """;

        var element = JsonDocument.Parse(json).RootElement;
        var result = NotionCardMapper.MapCard(element);

        Assert.NotNull(result);
        Assert.Equal("abc-123", result.Id);
        Assert.Equal("Test Card", result.Nome);
        Assert.Equal("Esta semana", result.Status);
        Assert.Equal(new DateTime(2026, 4, 18), result.Prazo!.Value);
        Assert.Equal("Alta", result.Prioridade);
        Assert.Equal(100.50m, result.Valor);
    }

    [Fact]
    public void MapCard_PropriedadeDataNula_RetornaCardComPrazoNulo()
    {
        var json = """
            {
                "id": "abc-123",
                "properties": {
                    "Name": { "title": [{ "plain_text": "Test Card" }] },
                    "Status": { "select": { "name": "Esta semana" } },
                    "Prazo": { "date": null }
                }
            }
            """;

        var element = JsonDocument.Parse(json).RootElement;
        var result = NotionCardMapper.MapCard(element);

        Assert.NotNull(result);
        Assert.Null(result.Prazo);
    }

    [Fact]
    public void MapCard_PropriedadeSelectNula_RetornaCardComPrioridadeNula()
    {
        var json = """
            {
                "id": "abc-123",
                "properties": {
                    "Name": { "title": [{ "plain_text": "Test Card" }] },
                    "Status": { "select": { "name": "Esta semana" } },
                    "Prioridade": { "select": null }
                }
            }
            """;

        var element = JsonDocument.Parse(json).RootElement;
        var result = NotionCardMapper.MapCard(element);

        Assert.NotNull(result);
        Assert.Null(result.Prioridade);
    }

    [Fact]
    public void MapCard_PropriedadesAusentes_RetornaNull()
    {
        var json = """{ "id": "abc-123" }""";

        var element = JsonDocument.Parse(json).RootElement;
        var result = NotionCardMapper.MapCard(element);

        Assert.Null(result);
    }

    [Fact]
    public void MapCard_CamposOpcionaisAusentes_RetornaCardComOpcionaisNulos()
    {
        var json = """
            {
                "id": "abc-123",
                "properties": {
                    "Name": { "title": [{ "plain_text": "Minimal Card" }] },
                    "Status": { "select": { "name": "Hoje" } }
                }
            }
            """;

        var element = JsonDocument.Parse(json).RootElement;
        var result = NotionCardMapper.MapCard(element);

        Assert.NotNull(result);
        Assert.Equal("Minimal Card", result.Nome);
        Assert.Null(result.Prazo);
        Assert.Null(result.Prioridade);
        Assert.Null(result.Valor);
    }
}
