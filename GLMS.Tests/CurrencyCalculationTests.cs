using Xunit;

namespace GLMS.Tests;

public class CurrencyCalculationTests
{
    [Fact]
    public void CurrencyConversion_100USD_at_18_50Rate_Returns_1850ZAR()
    {
        decimal usd = 100m;
        decimal rate = 18.50m;
        decimal expectedZar = 1850m;

        decimal actualZar = usd * rate;

        Assert.Equal(expectedZar, actualZar);
    }

    [Fact]
    public void CurrencyConversion_ZeroUSD_Returns_ZeroZAR()
    {
        decimal usd = 0m;
        decimal rate = 18.50m;

        decimal actualZar = usd * rate;

        Assert.Equal(0m, actualZar);
    }

    [Fact]
    public void CurrencyConversion_NegativeUSD_Returns_NegativeZAR()
    {
        decimal usd = -50m;
        decimal rate = 18.50m;

        decimal actualZar = usd * rate;

        Assert.Equal(-925m, actualZar);
    }
}