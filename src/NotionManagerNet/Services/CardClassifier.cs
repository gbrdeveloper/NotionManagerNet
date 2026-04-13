namespace NotionManagerNet.Services;

using NotionManagerNet.Models;

public sealed class CardClassifier
{
    private readonly TimeProvider _timeProvider;

    public CardClassifier(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public static readonly HashSet<string> IgnoredStatuses =
    [
        "Completo",
        "Em andamento",
        "Lembrete",
    ];

    public string Classify(TaskCard card)
    {
        if (card.Prazo is null)
            return card.Status;

        if (IgnoredStatuses.Contains(card.Status))
            return card.Status;

        var today = _timeProvider.GetUtcNow().Date;
        var prazo = card.Prazo.Value.Date;
        var diff = (prazo - today).Days;

        return diff switch
        {
            < 0 => "Atrasado",
            0 => "Hoje",
            _ when prazo <= ProximoSabado(today) => "Esta semana",
            _ when prazo <= SabadoDaProximaSemana(today) => "Próxima semana",
            _ when prazo.Month == today.Month && prazo.Year == today.Year => "Este mês",
            _ when prazo.Month == today.AddMonths(1).Month
                    && prazo.Year == today.AddMonths(1).Year => "Próximo mês",
            _ => card.Status,
        };

        static DateTime ProximoSabado(DateTime data)
        {
            var diasAteSabado = ((int)DayOfWeek.Saturday - (int)data.DayOfWeek + 7) % 7;
            diasAteSabado = diasAteSabado == 0 ? 7 : diasAteSabado;
            return data.AddDays(diasAteSabado);
        }

        static DateTime SabadoDaProximaSemana(DateTime data)
        {
            return ProximoSabado(data).AddDays(7);
        }
    }
}
