namespace NotionManagerNet.Tests;

using NotionManagerNet.Models;
using NotionManagerNet.Services;

public sealed class CardClassifierTests
{
    private static readonly DateTime FixedToday = new(2026, 4, 13);
    private readonly CardClassifier _classifier = new(new FixedTimeProvider(FixedToday));

    [Fact]
    public void Classify_PrazoNulo_RetornaStatusAtual()
    {
        var card = new TaskCard("1", "Card teste", "Lembrete", null);
        Assert.Equal("Lembrete", _classifier.Classify(card));
    }

    [Theory]
    [InlineData("Completo")]
    [InlineData("Em andamento")]
    [InlineData("Lembrete")]
    public void Classify_StatusIgnorado_RetornaStatusAtual(string status)
    {
        var card = new TaskCard("1", "Card teste", status, FixedToday.AddDays(1));
        Assert.Equal(status, _classifier.Classify(card));
    }

    [Fact]
    public void Classify_PrazoAnteriorAHoje_RetornaAtrasado()
    {
        var card = new TaskCard("1", "Card teste", "Esta semana", FixedToday.AddDays(-1));
        Assert.Equal("Atrasado", _classifier.Classify(card));
    }

    [Fact]
    public void Classify_PrazoEHoje_RetornaHoje()
    {
        var card = new TaskCard("1", "Card teste", "Próximo mês", FixedToday);
        Assert.Equal("Hoje", _classifier.Classify(card));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Classify_PrazoNestaSemana_RetornaEstaSemana(int daysAhead)
    {
        var card = new TaskCard("1", "Card teste", "Próximo mês", FixedToday.AddDays(daysAhead));
        Assert.Equal("Esta semana", _classifier.Classify(card));
    }

    [Theory]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(12)]
    public void Classify_PrazoNaProximaSemana_RetornaProximaSemana(int daysAhead)
    {
        var card = new TaskCard("1", "Card teste", "Este mês", FixedToday.AddDays(daysAhead));
        Assert.Equal("Próxima semana", _classifier.Classify(card));
    }

    [Theory]
    [InlineData(13)]
    [InlineData(17)]
    public void Classify_PrazoNestesMes_RetornaEsteMes(int daysAhead)
    {
        var card = new TaskCard("1", "Card teste", "Próximo mês", FixedToday.AddDays(daysAhead));
        Assert.Equal("Este mês", _classifier.Classify(card));
    }

    [Fact]
    public void Classify_PrazoNoProximoMes_RetornaProximoMes()
    {
        var card = new TaskCard("1", "Card teste", "Este mês", new DateTime(2026, 5, 15));
        Assert.Equal("Próximo mês", _classifier.Classify(card));
    }

    [Fact]
    public void Classify_PrazoAlemDoProximoMes_RetornaStatusAtual()
    {
        var card = new TaskCard("1", "Card teste", "Próximo mês", FixedToday.AddMonths(2));
        Assert.Equal("Próximo mês", _classifier.Classify(card));
    }
}

internal sealed class FixedTimeProvider(DateTime fixedDate) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(fixedDate, TimeSpan.Zero);
}
