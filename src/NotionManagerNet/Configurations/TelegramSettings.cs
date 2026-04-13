namespace NotionManagerNet.Configurations;

public sealed class TelegramSettings
{
    public string BotToken { get; init; }
    public string ChatId { get; init; }

    public TelegramSettings(string botToken, string chatId)
    {
        BotToken = !string.IsNullOrWhiteSpace(botToken)
            ? botToken
            : throw new ArgumentException("BotToken é obrigatório.", nameof(botToken));

        ChatId = !string.IsNullOrWhiteSpace(chatId)
            ? chatId
            : throw new ArgumentException("ChatId é obrigatório.", nameof(chatId));
    }
}
