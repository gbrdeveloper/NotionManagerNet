namespace NotionManagerNet.Tests;

using NotionManagerNet.Models;
using NotionManagerNet.Services;

public sealed class TelegramServiceTests
{
    private static readonly DateTime FixedNow = new(2026, 4, 13, 10, 0, 0);

    [Fact]
    public void MontarMensagem_SemAtualizacoes_ExibeMensagemVazia()
    {
        var mensagem = TelegramService.MontarMensagem([], 5, 0, FixedNow);

        Assert.Contains("Nenhum card precisou ser atualizado", mensagem);
        Assert.Contains("5 processados", mensagem);
    }

    [Fact]
    public void MontarMensagem_ComAtualizacoes_ListaDetalhesDoCard()
    {
        var card = new TaskCard("1", "Tarefa teste", "Este mês", new DateTime(2026, 4, 18));
        List<(TaskCard, string, string)> atualizacoes = [(card, "Este mês", "Esta semana")];

        var mensagem = TelegramService.MontarMensagem(atualizacoes, 1, 0, FixedNow);

        Assert.Contains("1 card atualizado", mensagem);
        Assert.Contains("Tarefa teste", mensagem);
        Assert.Contains("Este mês", mensagem);
        Assert.Contains("Esta semana", mensagem);
        Assert.Contains("→", mensagem);
    }

    [Fact]
    public void MontarMensagem_NomeComCaracteresHtml_EscapeaCaracteres()
    {
        var card = new TaskCard("1", "Task <bold> & more", "Este mês", new DateTime(2026, 4, 18));
        List<(TaskCard, string, string)> atualizacoes = [(card, "Este mês", "Esta semana")];

        var mensagem = TelegramService.MontarMensagem(atualizacoes, 1, 0, FixedNow);

        Assert.DoesNotContain("<bold>", mensagem);
        Assert.Contains("Task &lt;bold&gt; &amp; more", mensagem);
    }

    [Fact]
    public void MontarMensagem_ComErros_ExibeQuantidadeDeErros()
    {
        var mensagem = TelegramService.MontarMensagem([], 3, 2, FixedNow);

        Assert.Contains("2 erros", mensagem);
    }

    [Fact]
    public void MontarMensagem_SemErros_NaoExibeContadorDeErros()
    {
        var mensagem = TelegramService.MontarMensagem([], 3, 0, FixedNow);

        Assert.DoesNotContain("erro", mensagem);
    }

    [Fact]
    public void MontarMensagem_SempreContemDataFormatada()
    {
        var mensagem = TelegramService.MontarMensagem([], 0, 0, FixedNow);

        Assert.Contains("13/04/2026", mensagem);
        Assert.Contains("10:00", mensagem);
    }

    [Fact]
    public void MontarMensagem_ComTransicaoDeStatus_ExibeStatusAnteriorENovo()
    {
        var card = new TaskCard("1", "Card urgente", "Este mês", new DateTime(2026, 4, 13));
        List<(TaskCard, string, string)> atualizacoes = [(card, "Este mês", "Atrasado")];

        var mensagem = TelegramService.MontarMensagem(atualizacoes, 1, 0, FixedNow);

        Assert.Contains("Este mês", mensagem);
        Assert.Contains("Atrasado", mensagem);
        Assert.Contains("→", mensagem);
    }
}
