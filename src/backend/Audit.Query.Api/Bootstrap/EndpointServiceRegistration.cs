using Audit.Application.Exports;
using Audit.Application.Queries;
using Audit.Query.Api.Endpoints.Requests;
using Audit.Infrastructure.Queries;
using Audit.Query.Api.Endpoints.Validation;
using Audit.Infrastructure.Queries.ReadSources;

namespace Audit.Query.Api.Bootstrap;

internal static class EndpointServiceRegistration
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped<ContractAuditExportService>();

        var timelineValidator = new TimelineQueryValidator();
        services.AddSingleton<IRequestValidator<SearchQuery>>(new SearchQueryValidator());
        services.AddSingleton<IRequestValidator<ITimelineFilterQuery>>(timelineValidator);
        services.AddSingleton<IRequestValidator<ExportQuery>>(
            new ExportQueryValidator(timelineValidator));

        services.AddKeyedScoped<IContractStore>(
            ContractAuditDataSource.Active,
            (provider, _) => new SqlContractStore(
                provider.GetRequiredService<ActiveContractReadSource>()));
        services.AddKeyedScoped<IContractStore>(
            ContractAuditDataSource.Archive,
            (provider, _) => new SqlContractStore(
                provider.GetRequiredService<ArchivedContractReadSource>()));
    }
}
