using System.Text;
using System.Text.Json;

namespace GLMS.Web.Services
{
    public class ContractApiServices
    {
        private readonly HttpClient _httpClient;
        private string _authToken = string.Empty;

        public ContractApiServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SetAuthToken(string token)
        {
            _authToken = token;
            System.Diagnostics.Debug.WriteLine($"Auth token set: {!string.IsNullOrEmpty(token)}");
        }

        private void AddAuthHeader()
        {
            if (!string.IsNullOrEmpty(_authToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
            }
        }

        // ==================== CONTRACT METHODS ====================

        public async Task<List<Contract>> GetContractsAsync(string? clientName = null, string? status = null)
        {
            AddAuthHeader();

            var url = "/api/contracts";
            var parameters = new List<string>();

            if (!string.IsNullOrEmpty(clientName))
                parameters.Add($"clientName={Uri.EscapeDataString(clientName)}");
            if (!string.IsNullOrEmpty(status))
                parameters.Add($"status={status}");

            if (parameters.Any())
                url += "?" + string.Join("&", parameters);

            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new List<Contract>();
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Contract>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Contract>();
        }

        public async Task<Contract?> GetContractByIdAsync(int id)
        {
            AddAuthHeader();

            var response = await _httpClient.GetAsync($"/api/contracts/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Contract>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<Contract?> CreateContractAsync(Contract contract)
        {
            AddAuthHeader();

            var content = new StringContent(
                JsonSerializer.Serialize(contract),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api/contracts", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Contract>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<Contract?> UpdateContractStatusAsync(int id, string status)
        {
            AddAuthHeader();

            var content = new StringContent(
                $"\"{status}\"",
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PatchAsync($"/api/contracts/{id}/status", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Contract>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        // ==================== SERVICE REQUEST METHODS ====================

        public async Task<List<ServiceRequest>> GetServiceRequestsAsync()
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync("/api/ServiceRequests");  // Capital 'S'

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return new List<ServiceRequest>();

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ServiceRequest>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ServiceRequest>();
        }

        public async Task<ServiceRequest?> GetServiceRequestByIdAsync(int id)
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync($"/api/ServiceRequests/{id}");  // Capital 'S'

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ServiceRequest>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<ServiceRequest?> GetServiceRequestsByContractIdAsync(int contractId)
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync($"/api/ServiceRequests/by-contract/{contractId}");  // Capital 'S'

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ServiceRequest>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<ServiceRequest?> CreateServiceRequestAsync(ServiceRequest serviceRequest)
        {
            AddAuthHeader();
            var content = new StringContent(
                JsonSerializer.Serialize(serviceRequest),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api/ServiceRequests", content);  // Capital 'S'

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ServiceRequest>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<ServiceRequest?> UpdateServiceRequestStatusAsync(int id, string status)
        {
            AddAuthHeader();
            var content = new StringContent($"\"{status}\"", Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"/api/ServiceRequests/{id}/status", content);  // Capital 'S'
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ServiceRequest>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<ServiceRequest?> DeleteServiceRequestAsync(int id)
        {
            AddAuthHeader();
            var response = await _httpClient.DeleteAsync($"/api/ServiceRequests/{id}");  // Capital 'S'

            if (!response.IsSuccessStatusCode)
                return null;

            return new ServiceRequest();
        }
    }

    // Contract Model - matches API response
    public class Contract
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // Service Request Model - matches API response
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