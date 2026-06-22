namespace Audit.Infrastructure.Queries;

internal static class SqlLikePattern
{
    public static string Contains(string value) => $"%{Escape(value)}%";

    private static string Escape(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("%", "\\%", StringComparison.Ordinal)
        .Replace("_", "\\_", StringComparison.Ordinal)
        .Replace("[", "\\[", StringComparison.Ordinal);
}
