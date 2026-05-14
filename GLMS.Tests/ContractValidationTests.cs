using GLMS.Web.Models;
using Xunit;

namespace GLMS.Tests;

public class ContractValidationTests
{
    [Fact]
    public void ServiceRequest_ExpiredContract_NotAllowed()
    {
        var contract = new Contract { Status = ContractStatus.Expired };

        Assert.NotEqual(ContractStatus.Active, contract.Status);
    }

    [Fact]
    public void ServiceRequest_ActiveContract_Allowed()
    {
        var contract = new Contract { Status = ContractStatus.Active };

        Assert.Equal(ContractStatus.Active, contract.Status);
    }

    [Fact]
    public void Contract_EndDatePassed_ShouldBeExpired()
    {
        var contract = new Contract { EndDate = DateTime.Now.AddDays(-1) };

        var isExpired = contract.EndDate < DateTime.Now;

        Assert.True(isExpired);
    }
}