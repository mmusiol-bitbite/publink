using Audit.Infrastructure.Persistence;
using Audit.Infrastructure.Persistence.Core;
using Audit.Processing.Worker.Bootstrap;

var builder = Host.CreateApplicationBuilder(args);

ProcessingWorkerServices.Register(builder.Services, builder.Configuration);

var host = builder.Build();

await host.Services.InitializeRequiredDatabasesAsync(
    host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping);

await host.RunAsync();
