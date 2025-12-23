using JobRunner.Domain.Interfaces;
using Microsoft.Extensions.Logging;


namespace JobRunner.Infrastructure.Handlers;

public class OpenMeteoForecastJobHandler : IJobHandler
{
    public string JobType => "OpenMeteoForecast";

    private readonly string _baseUrl = "https://api.open-meteo.com/v1/forecast";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IExternalApiResultStore _externalApiResultStore;
    private readonly ILogger<OpenMeteoForecastJobHandler> _logger;

    public OpenMeteoForecastJobHandler(IHttpClientFactory httpClientFactory,IExternalApiResultStore externalApiResultStore, ILogger<OpenMeteoForecastJobHandler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _externalApiResultStore = externalApiResultStore;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid runId, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("open-meteo");
        var url = $"{_baseUrl}?latitude=33.4484&longitude=-112.0740&current=temperature_2m";

        _logger.LogInformation("Open-Meteo run started. RunId={RunId}", runId);

        HttpResponseMessage response;
        string? body = null;

        try
        {
            response = await client.GetAsync(url, ct);
            body = await response.Content.ReadAsStringAsync(ct);

            string? error = response.IsSuccessStatusCode
                ? null
                : $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";

            await _externalApiResultStore.SaveAsync(
                runId,
                provider: "open-meteo",
                httpStatusCode: (int)response.StatusCode,
                responseBody: body,
                error: error,
                nowUtc: DateTime.UtcNow,
                ct
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Open-Meteo request failed with status {(int)response.StatusCode}");
            }

            _logger.LogInformation("Open-Meteo run succeeded. RunId={RunId}", runId);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            await _externalApiResultStore.SaveAsync(
                runId,
                provider: "open-meteo",
                httpStatusCode: 0,
                responseBody: body,
                error: ex.ToString(),
                nowUtc: DateTime.UtcNow,
                ct
            );

            throw;
        }
    }

}
