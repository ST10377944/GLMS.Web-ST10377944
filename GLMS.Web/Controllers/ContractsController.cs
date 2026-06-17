using Microsoft.AspNetCore.Mvc;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ContractApiServices _apiService;
        private readonly TokenStorageService _tokenStorage;

        public ContractsController(ContractApiServices apiService, TokenStorageService tokenStorage)
        {
            _apiService = apiService;
            _tokenStorage = tokenStorage;
        }

        // GET: Contracts
        public async Task<IActionResult> Index(string sid)
        {
            System.Diagnostics.Debug.WriteLine("=== CONTRACTS INDEX HIT ===");
            System.Diagnostics.Debug.WriteLine($"Session ID received: {sid ?? "NULL"}");

            if (string.IsNullOrEmpty(sid))
            {
                System.Diagnostics.Debug.WriteLine("No session ID, redirecting to login");
                return RedirectToAction("Index", "Login");
            }

            // Get token from server-side storage
            var token = _tokenStorage.GetToken(sid);

            if (string.IsNullOrEmpty(token))
            {
                System.Diagnostics.Debug.WriteLine("No token found for this session ID, redirecting to login");
                return RedirectToAction("Index", "Login");
            }

            // Set the token for API calls
            _apiService.SetAuthToken(token);

            // Fetch contracts from API
            var contracts = await _apiService.GetContractsAsync();
            System.Diagnostics.Debug.WriteLine($"Got {contracts?.Count ?? 0} contracts");

            // Pass session ID to view for subsequent requests
            ViewBag.SessionId = sid;

            return View(contracts);
        }

        // GET: Contracts/Create
        public IActionResult Create(string sid)
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
            ViewBag.SessionId = sid;
            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract, string sid)
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
                await _apiService.CreateContractAsync(contract);
                return RedirectToAction(nameof(Index), new { sid = sid });
            }
            ViewBag.SessionId = sid;
            return View(contract);
        }

        // POST: Contracts/Approve/5
        [HttpPost]
        public async Task<IActionResult> Approve(int id, string sid)
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

            await _apiService.UpdateContractStatusAsync(id, "Approved");
            return RedirectToAction(nameof(Index), new { sid = sid });
        }

        // POST: Contracts/Decline/5
        [HttpPost]
        public async Task<IActionResult> Decline(int id, string sid)
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

            await _apiService.UpdateContractStatusAsync(id, "Declined");
            return RedirectToAction(nameof(Index), new { sid = sid });
        }

        // GET: Contracts/CreateServiceRequestFromContract
        // This redirects to ServiceRequests Create with the contract pre-selected
        public IActionResult CreateServiceRequest(int contractId, string sid)
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

            // Redirect to ServiceRequests Create with contractId parameter
            return RedirectToAction("Create", "ServiceRequests", new { sid = sid, contractId = contractId });
        }

        public IActionResult Logout(string sid)
        {
            if (!string.IsNullOrEmpty(sid))
            {
                _tokenStorage.RemoveToken(sid);
            }
            return RedirectToAction("Index", "Login");
        }
    }
}