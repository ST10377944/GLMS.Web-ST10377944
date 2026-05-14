using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers;

public class ServiceRequestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrencyService _currencyService;

    public ServiceRequestsController(ApplicationDbContext context, ICurrencyService currencyService)
    {
        _context = context;
        _currencyService = currencyService;
    }

    // GET: ServiceRequests
    public async Task<IActionResult> Index()
    {
        var serviceRequests = await _context.ServiceRequests
            .Include(s => s.Contract)
            .ThenInclude(c => c!.Client)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return View(serviceRequests);
    }

    // GET: ServiceRequests/Create
    public async Task<IActionResult> Create()
    {
        // Only show ACTIVE contracts for new service requests
        var activeContracts = await _context.Contracts
            .Include(c => c.Client)
            .Where(c => c.Status == ContractStatus.Active)
            .ToListAsync();

        ViewBag.Contracts = activeContracts;
        ViewBag.CurrentRate = await _currencyService.GetUsdToZarRateAsync();

        return View();
    }

    // POST: ServiceRequests/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequest serviceRequest)
    {
        // Validate that the contract exists and is ACTIVE
        var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);
        if (contract == null)
        {
            ModelState.AddModelError("ContractId", "Selected contract does not exist.");
        }
        else if (contract.Status != ContractStatus.Active)
        {
            ModelState.AddModelError("ContractId", "Service requests can only be created against ACTIVE contracts.");
        }

        // Get current exchange rate and calculate ZAR amount
        var rate = await _currencyService.GetUsdToZarRateAsync();
        serviceRequest.ExchangeRateUsed = rate;
        serviceRequest.AmountZAR = serviceRequest.AmountUSD * rate;
        serviceRequest.CreatedAt = DateTime.UtcNow;
        serviceRequest.Status = RequestStatus.Pending;

        if (ModelState.IsValid)
        {
            _context.Add(serviceRequest);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Service request created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // If we got here, something failed - repopulate the form
        var activeContracts = await _context.Contracts
            .Include(c => c.Client)
            .Where(c => c.Status == ContractStatus.Active)
            .ToListAsync();
        ViewBag.Contracts = activeContracts;
        ViewBag.CurrentRate = rate;

        return View(serviceRequest);
    }

    // API endpoint for AJAX currency calculation
    [HttpGet]
    public async Task<IActionResult> GetExchangeRate()
    {
        var rate = await _currencyService.GetUsdToZarRateAsync();
        return Json(new { rate });
    }

    // GET: ServiceRequests/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var serviceRequest = await _context.ServiceRequests
            .Include(s => s.Contract)
            .ThenInclude(c => c!.Client)
            .FirstOrDefaultAsync(s => s.ServiceRequestId == id);

        if (serviceRequest == null)
        {
            return NotFound();
        }

        return View(serviceRequest);
    }
}