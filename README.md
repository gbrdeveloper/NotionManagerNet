# NotionManagerNet

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-13-239120?style=flat-square&logo=csharp&logoColor=white)
![Build](https://img.shields.io/badge/build-passing-brightgreen?style=flat-square&logo=github-actions&logoColor=white)
![Tests](https://img.shields.io/badge/tests-28%20passed-brightgreen?style=flat-square&logo=xunit&logoColor=white)
![Coverage](https://img.shields.io/badge/coverage-domain%20100%25-brightgreen?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)

Console app em .NET que monitora um banco de dados do **Notion** e classifica automaticamente os cards por prazo, atualizando o status de cada um e enviando um resumo diário pelo **Telegram**.

---

## Como funciona

1. Busca todos os cards do banco de dados configurado no Notion via API REST
2. Para cada card elegível (com prazo e status não ignorado), classifica o status com base na distância até o prazo
3. Atualiza o status no Notion caso tenha mudado
4. Envia um resumo com todas as atualizações para um chat do Telegram

### Regras de classificação

| Prazo                   | Status atribuído   |
|-------------------------|--------------------|
| Antes de hoje           | Atrasado           |
| Hoje                    | Hoje               |
| Até o próximo sábado    | Esta semana        |
| Até o sábado seguinte   | Próxima semana     |
| Ainda neste mês         | Este mês           |
| No próximo mês          | Próximo mês        |

> Os status `Completo`, `Em andamento` e `Lembrete` são ignorados — cards com esses status nunca têm o prazo reclassificado.

---

## Tecnologias

- [.NET 10](https://dotnet.microsoft.com/)
- [Notion API](https://developers.notion.com/)
- [Telegram Bot API](https://core.telegram.org/bots/api)
- [xUnit](https://xunit.net/)

---

## Configuração

As credenciais são lidas via **variáveis de ambiente** ou **User Secrets** (desenvolvimento local).

| Variável               | Descrição                                      |
|------------------------|------------------------------------------------|
| `NOTION_TOKEN`         | Token de integração do Notion                  |
| `NOTION_DATABASE_ID`   | ID do banco de dados do Notion                 |
| `TELEGRAM_BOT_TOKEN`   | Token do bot do Telegram                       |
| `TELEGRAM_CHAT_ID`     | ID do chat que receberá o resumo               |

### Desenvolvimento local com User Secrets

```bash
dotnet user-secrets set "NOTION_TOKEN" "secret_..."
dotnet user-secrets set "NOTION_DATABASE_ID" "..."
dotnet user-secrets set "TELEGRAM_BOT_TOKEN" "..."
dotnet user-secrets set "TELEGRAM_CHAT_ID" "..."
```

---

## Executando

```bash
dotnet run --project src/NotionManagerNet/NotionManagerNet.csproj
```

---

## Testes

```bash
dotnet test
```

Os testes cobrem:

- `CardClassifier` — todas as regras de classificação por prazo com data fixa (determinístico)
- `NotionCardMapper` — mapeamento de JSON da API para o modelo de domínio
- `TelegramService` — montagem da mensagem, escape de HTML e pluralização

---

## Estrutura do projeto

```
src/
  NotionManagerNet/
    Configurations/   # NotionSettings, TelegramSettings
    Interfaces/       # INotionService, ITelegramService
    Mappers/          # NotionCardMapper
    Models/           # TaskCard
    Services/         # NotionService, TelegramService, CardClassifier
    Program.cs
tests/
  NotionManagerNet.Tests/
    CardClassifierTests.cs
    NotionCardMapperTests.cs
    TelegramServiceTests.cs
```
