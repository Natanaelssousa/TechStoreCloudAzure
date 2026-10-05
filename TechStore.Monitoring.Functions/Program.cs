using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddHttpClient("ProductsApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(90);
});

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.Configure<LoggerFilterOptions>(options =>
{
    options.Rules.Add(new LoggerFilterRule(
        providerName:
            "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider",
        categoryName: "TechStore.Monitoring.Functions",
        logLevel: LogLevel.Information,
        filter: null));
});

builder.Build().Run();