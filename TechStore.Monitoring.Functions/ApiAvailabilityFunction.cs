using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TechStore.Monitoring.Functions;

public sealed class ApiAvailabilityFunction
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ApiAvailabilityFunction> _logger;
    private readonly Uri _apiUrl;

    public ApiAvailabilityFunction(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ApiAvailabilityFunction> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        var configuredUrl = configuration["ProductsApiUrl"];

        if (!Uri.TryCreate(configuredUrl, UriKind.Absolute, out var apiUrl)
            || apiUrl.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                "Configure ProductsApiUrl com uma URL HTTPS válida.");
        }

        _apiUrl = apiUrl;
    }

    [Function("CheckProductsApi")]
    public async Task Run(
        [TimerTrigger("%MonitoringSchedule%")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var client = _httpClientFactory.CreateClient("ProductsApi");

            using var response = await client.GetAsync(
                _apiUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Verificação da API falhou. StatusHttp={StatusHttp}; " +
                    "DuracaoMs={DuracaoMs}",
                    (int)response.StatusCode,
                    stopwatch.ElapsedMilliseconds);

                response.EnsureSuccessStatusCode();
            }

            _logger.LogInformation(
                "API disponível. StatusHttp={StatusHttp}; " +
                "DuracaoMs={DuracaoMs}",
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            _logger.LogError(
                exception,
                "Erro na verificação da API. DuracaoMs={DuracaoMs}",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}