using Audit.Archival.Worker.Bootstrap;
using Audit.Infrastructure.Persistence.Core;

var builder = Host.CreateApplicationBuilder(args);

ArchivalWorkerServices.Register(builder.Services, builder.Configuration);

var host = builder.Build();

await host.Services.InitializeRequiredDatabasesAsync(
    host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);

await host.RunAsync();
