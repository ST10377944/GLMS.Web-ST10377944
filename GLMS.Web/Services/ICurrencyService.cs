namespace GLMS.Web.Services;

public interface ICurrencyService
{
    Task<decimal> GetUsdToZarRateAsync();
}