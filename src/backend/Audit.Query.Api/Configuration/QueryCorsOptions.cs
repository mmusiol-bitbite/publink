using System.Diagnostics.CodeAnalysis;

namespace Audit.Query.Api.Configuration;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by the options configuration binder.")]
internal sealed class QueryCorsOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; init; } = [];
}
