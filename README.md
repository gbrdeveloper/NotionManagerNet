# NotionManagerNet

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-13-239120?style=flat-square&logo=csharp&logoColor=white)
![Build](https://github.com/gbrdeveloper/NotionManagerNet/actions/workflows/notion-manager.yml/badge.svg?style=flat-square)
![Tests](https://img.shields.io/badge/tests-28%20passed-brightgreen?style=flat-square&logo=xunit&logoColor=white)
![Coverage](https://img.shields.io/badge/coverage-domain%20100%25-brightgreen?style=flat-square)
![Last Run](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/gbrdeveloper/9e1250a9dfc48a851e250d0645dc8fd1/raw/notion-manager-status.json)
![Schedule](https://img.shields.io/badge/schedule-mon--fri%2008h%20BRT-blue?style=flat-square&logo=googlecalendar&logoColor=white)
![Notion](https://img.shields.io/badge/Notion-API-000000?style=flat-square&logo=notion&logoColor=white)
![Telegram](https://img.shields.io/badge/Telegram-Bot-26A5E4?style=flat-square&logo=telegram&logoColor=white)
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

---

## Template Notion

Acesse o template utilizado neste projeto e duplique para o seu Notion:

[![Notion Template](https://img.shields.io/badge/Notion-Abrir%20Template-000000?style=flat-square&logo=notion&logoColor=white)](https://www.notion.so/gbrdev/341d2eefd04a800a8b67f0b0301fc496?v=341d2eefd04a81a5b0a1000cdab018c0&source=copy_link)

> Clique em **Duplicate** no canto superior direito para copiar o template para o seu workspace.
