using Audit.Infrastructure.Persistence;
using Audit.Infrastructure.Persistence.Core;
using Audit.Query.Api.Bootstrap;

var builder = WebApplication.CreateBuilder(args);

QueryApiServices.Register(builder.Services, builder.Configuration);

var app = builder.Build();

await app.Services.InitializeRequiredDatabasesAsync(app.Lifetime.ApplicationStopping);
QueryApiStartup.ConfigurePipeline(app);
QueryApiStartup.MapEndpoints(app);

await app.RunAsync();

public partial class Program;
