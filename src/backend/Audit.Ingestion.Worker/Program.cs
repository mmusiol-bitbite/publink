using Audit.Infrastructure.Persistence.Core;
using Audit.Ingestion.Worker.Bootstrap;

var builder = Host.CreateApplicationBuilder(args);

IngestionWorkerServices.Register(builder.Services, builder.Configuration);

var host = builder.Build();

await host.Services.InitializeRequiredDatabasesAsync(
    host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);

await host.RunAsync();
