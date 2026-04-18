using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotionManagerNet.Configurations;
using NotionManagerNet.Interfaces;
using NotionManagerNet.Services;

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .Build();

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Warning);
    builder.AddFilter("NotionManagerNet.Services", LogLevel.Information);
    builder.AddFilter("Program", LogLevel.Information);
});
services.AddHttpClient(
    "Notion",
    (sp, client) =>
    {
        var notionSettings = sp.GetRequiredService<NotionSettings>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            notionSettings.Token
        );
        client.DefaultRequestHeaders.Add("Notion-Version", "2022-06-28");
    }
);
services.AddSingleton(TimeProvider.System);
services.AddSingleton<CardClassifier>();

var token = configuration["NOTION_TOKEN"];
var databaseId = configuration["NOTION_DATABASE_ID"];
var telegramToken = configuration["TELEGRAM_BOT_TOKEN"];
var telegramChatId = configuration["TELEGRAM_CHAT_ID"];

if (string.IsNullOrWhiteSpace(telegramToken) || string.IsNullOrWhiteSpace(telegramChatId))
{
    var tempLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Program");
    tempLogger.LogError("TELEGRAM_BOT_TOKEN e TELEGRAM_CHAT_ID são obrigatórios.");
    return 1;
}

services.AddHttpClient("Telegram");
services.AddSingleton(new TelegramSettings(telegramToken, telegramChatId));
services.AddSingleton<ITelegramService, TelegramService>();

if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(databaseId))
{
    var tempLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Program");
    tempLogger.LogError("NOTION_TOKEN e NOTION_DATABASE_ID são obrigatórios.");
    return 1;
}

services.AddSingleton(new NotionSettings(token, databaseId));
services.AddSingleton<INotionService, NotionService>();

var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Program");

logger.LogInformation("NotionManager iniciado!");

try
{
    var notionService = provider.GetRequiredService<INotionService>();
    var updatedCount = await notionService.ExecutarAsync();

    Console.WriteLine($"UPDATED_COUNT={updatedCount}");

    logger.LogInformation("NotionManager finalizado com sucesso!");
    return 0;
}
catch (Exception ex)
{
    logger.LogError(ex, "Erro fatal ao executar NotionManager.");
    return 1;
}
