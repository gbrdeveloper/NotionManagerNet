namespace NotionManagerNet.Configurations;

public sealed class NotionSettings(string token, string databaseId)
{
    public string Token { get; } =
        !string.IsNullOrWhiteSpace(token)
            ? token
            : throw new ArgumentException("Token é obrigatório.", nameof(token));

    public string DatabaseId { get; } =
        !string.IsNullOrWhiteSpace(databaseId)
            ? databaseId
            : throw new ArgumentException("DatabaseId é obrigatório.", nameof(databaseId));
}
