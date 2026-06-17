using Microsoft.AspNetCore.Mvc;
using GLMS.Web.Services;
using System.Text.Json;

namespace GLMS.Web.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ContractApiServices _apiService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly TokenStorageService _tokenStorage;

        public ServiceRequestsController(ContractApiServices apiService, IConfiguration configuration, TokenStorageService tokenStorage)
        {
            _apiService = apiService;
            _configuration = configuration;
            _tokenStorage = tokenStorage;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_configuration["ApiBaseUrl"]);
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index(string sid)
        {
            System.Diagnostics.Debug.WriteLine("=== SERVICE REQUESTS INDEX HIT ===");
            System.Diagnostics.Debug.WriteLine($"Session ID received: {sid ?? "NULL"}");

            if (string.IsNullOrEmpty(sid))
            {
                return RedirectToAction("Index", "Login");
            }

            var token = _tokenStorage.GetToken(sid);
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Login");
            }

            _apiService.SetAuthToken(token);

            // Get service requests from API
            var serviceRequests = await _apiService.GetServiceRequestsAsync();
            ViewBag.SessionId = sid;

            return View(serviceRequests);
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create(string sid, int? contractId)
        {
            if (string.IsNullOrEmpty(sid))
            {
                return RedirectToAction("Index", "Login");
            }

            var token = _tokenStorage.GetToken(sid);
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Login");
            }

            _apiService.SetAuthToken(token);

            // Get only active contracts (Status = "Approved")
            var allContracts = await _apiService.GetContractsAsync();
            var activeContracts = allContracts?.Where(c => c.Status == "Approved").ToList() ?? new List<Contract>();

            ViewBag.Contracts = activeContracts;
            ViewBag.SessionId = sid;
            ViewBag.SelectedContractId = contractId ?? 0;

            // Get current exchange rate
            var rate = await GetExchangeRateAsync();
            ViewBag.CurrentRate = rate;

            var viewModel = new ServiceRequestViewModel();
            if (contractId.HasValue && contractId.Value > 0)
            {
                viewModel.ContractId = contractId.Value;
            }

            return View(viewModel);
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestViewModel serviceRequest, string sid)
        {
            if (string.IsNullOrEmpty(sid))
            {
                return RedirectToAction("Index", "Login");
            }

            var token = _tokenStorage.GetToken(sid);
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Login");
            }

            _apiService.SetAuthToken(token);

            if (ModelState.IsValid)
            {
                try
                {
                    // Create the service request
                    var newRequest = new ServiceRequest
                    {
                        ContractId = serviceRequest.ContractId,
                        Description = serviceRequest.Description,
                        AmountUSD = serviceRequest.AmountUSD,
                        AmountZAR = serviceRequest.AmountUSD * await GetExchangeRateAsync(),
                        ExchangeRateUsed = await GetExchangeRateAsync(),
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _apiService.CreateServiceRequestAsync(newRequest);
                    TempData["Success"] = "Service request created successfully!";
                    return RedirectToAction(nameof(Index), new { sid = sid });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // Repopulate form
            var allContracts = await _apiService.GetContractsAsync();
            var activeContracts = allContracts?.Where(c => c.Status == "Approved").ToList() ?? new List<Contract>();
            ViewBag.Contracts = activeContracts;
            ViewBag.CurrentRate = await GetExchangeRateAsync();
            ViewBag.SessionId = sid;
            ViewBag.SelectedContractId = serviceRequest.ContractId;

            return View(serviceRequest);
        }

        // GET: ServiceRequests/Details/5
        public async Task<IActionResult> Details(int id, string sid)
        {
            if (string.IsNullOrEmpty(sid))
            {
                return RedirectToAction("Index", "Login");
            }

            var token = _tokenStorage.GetToken(sid);
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Login");
            }

            _apiService.SetAuthToken(token);

            // Get service request from API
            var serviceRequest = await _apiService.GetServiceRequestByIdAsync(id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            ViewBag.SessionId = sid;
            return View(serviceRequest);
        }

        public IActionResult Logout(string sid)
        {
            if (!string.IsNullOrEmpty(sid))
            {
                _tokenStorage.RemoveToken(sid);
            }
            return RedirectToAction("Index", "Login");
        }

        private async Task<decimal> GetExchangeRateAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://api.exchangerate-api.com/v4/latest/USD");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var zarRate = doc.RootElement.GetProperty("rates").GetProperty("ZAR").GetDecimal();
                    return zarRate;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting exchange rate: {ex.Message}");
            }
            return 19.50m;
        }
    }

    public class ServiceRequestViewModel
    {
        public int ServiceRequestId { get; set; }
        public int ContractId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal AmountUSD { get; set; }
        public decimal AmountZAR { get; set; }
        public decimal ExchangeRateUsed { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}