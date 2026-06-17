using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace ContractManagement.Tests
{
    public class ContractApiIntegrationTests : IDisposable
    {
        private readonly HttpClient _client;
        private string _token = string.Empty;

        public ContractApiIntegrationTests()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            _client = new HttpClient(handler);
            _client.BaseAddress = new Uri("https://localhost:7208"); // Make sure this matches your API port
        }

        public void Dispose() => _client?.Dispose();

        private async Task<bool> IsApiRunning()
        {
            try
            {
                var response = await _client.GetAsync("/api/contracts");
                return response.StatusCode == HttpStatusCode.Unauthorized ||
                       response.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }

        private async Task AuthenticateAsync()
        {
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
                new { username = "admin", password = "password" });

            loginResponse.EnsureSuccessStatusCode();

            var json = await loginResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            _token = doc.RootElement.GetProperty("token").GetString();

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        }

        // ==================== AUTH TESTS ====================

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            if (!await IsApiRunning())
            {
                Assert.True(false, $"API is not running at {_client.BaseAddress}. Please start the API first.");
                return;
            }

            var loginData = new { username = "admin", password = "password" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("token", json.ToLower());
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            if (!await IsApiRunning())
            {
                Assert.True(false, $"API is not running at {_client.BaseAddress}. Please start the API first.");
                return;
            }

            var loginData = new { username = "wrong", password = "wrong" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ==================== CONTRACT TESTS ====================

        [Fact]
        public async Task GetAllContracts_WithoutToken_ReturnsUnauthorized()
        {
            if (!await IsApiRunning())
            {
                Assert.True(false, $"API is not running at {_client.BaseAddress}. Please start the API first.");
                return;
            }

            var response = await _client.GetAsync("/api/contracts");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAllContracts_WithValidToken_ReturnsOkAndContracts()
        {
            if (!await IsApiRunning())
            {
                Assert.True(false, $"API is not running at {_client.BaseAddress}. Please start the API first.");
                return;
            }

            await AuthenticateAsync();
            var response = await _client.GetAsync("/api/contracts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>();
            Assert.NotNull(contracts);
            Assert.NotEmpty(contracts);
        }

        [Fact]
        public async Task CreateContract_WithValidData_ReturnsCreated()
        {
            if (!await IsApiRunning())
            {
                Assert.True(false, $"API is not running at {_client.BaseAddress}. Please start the API first.");
                return;
            }

            await AuthenticateAsync();
            var newContract = new Contract
            {
                ClientName = "Integration Test Client",
                Description = "Test Contract",
                Amount = 5000
            };

            var response = await _client.PostAsJsonAsync("/api/contracts", newContract);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdContract = await response.Content.ReadFromJsonAsync<Contract>();
            Assert.NotNull(createdContract);
            Assert.Equal("Pending", createdContract?.Status);
        }

        [Fact]
        public async Task CreateContract_WithoutClientName_ReturnsBadRequest()
        {
            if (!await IsApiRunning())
            {
                Assert.True(false, $"API is not running at {_client.BaseAddress}. Please start the API first.");
                return;
            }

            await AuthenticateAsync();
            var invalidContract = new Contract
            {
                ClientName = "",
                Description = "Invalid Contract",
                Amount = 5000
            };

            var response = await _client.PostAsJsonAsync("/api/contracts", invalidContract);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateContractStatus_WithValidId_ReturnsOk()
        {
            if (!await IsApiRunning())
            {
                Assert.True(false, $"API is not running at {_client.BaseAddress}. Please start the API first.");
                return;
            }

            await AuthenticateAsync();

            var newContract = new Contract { ClientName = "Status Test", Description = "Test", Amount = 100 };
            var createResponse = await _client.PostAsJsonAsync("/api/contracts", newContract);
            var created = await createResponse.Content.ReadFromJsonAsync<Contract>();

            var content = new StringContent("\"Approved\"", Encoding.UTF8, "application/json");
            var response = await _client.PatchAsync($"/api/contracts/{created?.Id}/status", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<Contract>();
            Assert.Equal("Approved", updated?.Status);
        }

        [Fact]
        public async Task UpdateContractStatus_WithInvalidId_ReturnsNotFound()
        {
            if (!await IsApiRunning())
            {
                Assert.True(false, $"API is not running at {_client.BaseAddress}. Please start the API first.");
                return;
            }

            await AuthenticateAsync();
            var content = new StringContent("\"Approved\"", Encoding.UTF8, "application/json");
            var response = await _client.PatchAsync("/api/contracts/99999/status", content);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetContracts_WithFilter_ReturnsFilteredResults()
        {
            if (!await IsApiRunning())
            {
                Assert.True(false, $"API is not running at {_client.BaseAddress}. Please start the API first.");
                return;
            }

            await AuthenticateAsync();
            var response = await _client.GetAsync("/api/contracts?status=Pending");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>();
            Assert.NotNull(contracts);
            foreach (var contract in contracts!)
            {
                Assert.Equal("Pending", contract.Status);
            }
        }

        [Fact]
        public async Task DeleteContract_WithValidId_ReturnsNoContent()
        {
            if (!await IsApiRunning())
            {
                Assert.True(false, $"API is not running at {_client.BaseAddress}. Please start the API first.");
                return;
            }

            await AuthenticateAsync();

            var newContract = new Contract { ClientName = "Delete Test", Description = "To be deleted", Amount = 100 };
            var createResponse = await _client.PostAsJsonAsync("/api/contracts", newContract);
            var created = await createResponse.Content.ReadFromJsonAsync<Contract>();

            var response = await _client.DeleteAsync($"/api/contracts/{created?.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        // ==================== SERVICE REQUEST TESTS ====================

        [Fact]
        public async Task GetAllServiceRequests_WithValidToken_ReturnsOk()
        {
            if (!await IsApiRunning())
            {
                Assert.True(false, $"API is not running at {_client.BaseAddress}. Please start the API first.");
                return;
            }

            await AuthenticateAsync();
            var response = await _client.GetAsync("/api/ServiceRequests");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var serviceRequests = await response.Content.ReadFromJsonAsync<List<ServiceRequest>>();
            Assert.NotNull(serviceRequests);
        }

        [Fact]
        public async Task CreateServiceRequest_AgainstApprovedContract_ReturnsCreated()
        {
            if (!await IsApiRunning())
            {
                Assert.True(false, $"API is not running at {_client.BaseAddress}. Please start the API first.");
                return;
            }

            await AuthenticateAsync();

            var newContract = new Contract { ClientName = "SR Test Client", Description = "Test", Amount = 1000 };
            var createResponse = await _client.PostAsJsonAsync("/api/contracts", newContract);
            var created = await createResponse.Content.ReadFromJsonAsync<Contract>();

            var approveContent = new StringContent("\"Approved\"", Encoding.UTF8, "application/json");
            await _client.PatchAsync($"/api/contracts/{created?.Id}/status", approveContent);

            var serviceRequest = new ServiceRequest
            {
                ContractId = created?.Id ?? 0,
                Description = "Test Service Request",
                AmountUSD = 500,
                AmountZAR = 9750,
                ExchangeRateUsed = 19.50m
            };

            var response = await _client.PostAsJsonAsync("/api/ServiceRequests", serviceRequest);
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CreateServiceRequest_AgainstPendingContract_ReturnsBadRequest()
        {
            if (!await IsApiRunning())
            {
                Assert.True(false, $"API is not running at {_client.BaseAddress}. Please start the API first.");
                return;
            }

            await AuthenticateAsync();

            var newContract = new Contract { ClientName = "SR Test Client 2", Description = "Test", Amount = 1000 };
            var createResponse = await _client.PostAsJsonAsync("/api/contracts", newContract);
            var created = await createResponse.Content.ReadFromJsonAsync<Contract>();

            var serviceRequest = new ServiceRequest
            {
                ContractId = created?.Id ?? 0,
                Description = "Test Service Request",
                AmountUSD = 500,
                AmountZAR = 9750,
                ExchangeRateUsed = 19.50m
            };

            var response = await _client.PostAsJsonAsync("/api/ServiceRequests", serviceRequest);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ==================== REMOVED: UpdateServiceRequestStatus_WithValidId_ReturnsOk ====================
        // This test was failing because the PATCH endpoint returned 404.
        // It has been removed as requested.
        // ==================== END REMOVED ====================

    }

    // Models for testing
    public class Contract
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ServiceRequest
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal AmountUSD { get; set; }
        public decimal AmountZAR { get; set; }
        public decimal ExchangeRateUsed { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Contract? Contract { get; set; }
    }
}