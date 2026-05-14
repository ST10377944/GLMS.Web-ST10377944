using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers;

public class ContractsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileValidationService _fileValidationService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ContractsController(
        ApplicationDbContext context,
        IFileValidationService fileValidationService,
        IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _fileValidationService = fileValidationService;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index(string? status, DateTime? startDateFrom, DateTime? startDateTo)
    {
        var query = _context.Contracts.Include(c => c.Client).AsQueryable();

        if (startDateFrom.HasValue)
            query = query.Where(c => c.StartDate >= startDateFrom.Value);
        if (startDateTo.HasValue)
            query = query.Where(c => c.StartDate <= startDateTo.Value);
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ContractStatus>(status, true, out var statusEnum))
            query = query.Where(c => c.Status == statusEnum);

        ViewBag.CurrentStatus = status;
        ViewBag.StartDateFrom = startDateFrom?.ToString("yyyy-MM-dd");
        ViewBag.StartDateTo = startDateTo?.ToString("yyyy-MM-dd");

        return View(await query.ToListAsync());
    }

    public IActionResult Create()
    {
        ViewData["Clients"] = _context.Clients.ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Contract contract, IFormFile? signedAgreement)
    {
        if (signedAgreement != null)
        {
            var validation = _fileValidationService.ValidatePdfFile(signedAgreement);
            if (!validation.IsValid)
                ModelState.AddModelError("signedAgreement", validation.ErrorMessage);
            else
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "contracts");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = $"{Guid.NewGuid()}_{signedAgreement.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await signedAgreement.CopyToAsync(stream);
                contract.SignedAgreementPath = $"/uploads/contracts/{uniqueFileName}";
            }
        }

        if (ModelState.IsValid)
        {
            _context.Add(contract);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["Clients"] = _context.Clients.ToList();
        return View(contract);
    }

    public async Task<IActionResult> DownloadPdf(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract?.SignedAgreementPath == null) return NotFound();

        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, contract.SignedAgreementPath.TrimStart('/'));
        if (!System.IO.File.Exists(filePath)) return NotFound();

        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(bytes, "application/pdf", Path.GetFileName(filePath));
    }
}