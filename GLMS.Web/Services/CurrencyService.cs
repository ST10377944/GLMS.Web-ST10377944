using System.Text.Json;

namespace GLMS.Web.Services;

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CurrencyService> _logger;
    private decimal? _cachedRate;
    private DateTime _cacheExpiry;

    public CurrencyService(HttpClient httpClient, ILogger<CurrencyService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<decimal> GetUsdToZarRateAsync()
    {
        if (_cachedRate.HasValue && DateTime.UtcNow < _cacheExpiry)
        {
            return _cachedRate.Value;
        }

        try
        {
            var response = await _httpClient.GetAsync("https://api.exchangerate-api.com/v4/latest/USD");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var rate = doc.RootElement.GetProperty("rates").GetProperty("ZAR").GetDecimal();

            _cachedRate = rate;
            _cacheExpiry = DateTime.UtcNow.AddHours(1);
            return rate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch exchange rate");
            return _cachedRate ?? 18.50m;
        }
    }
}