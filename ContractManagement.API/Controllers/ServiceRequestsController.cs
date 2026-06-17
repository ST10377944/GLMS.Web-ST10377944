using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractManagement.API.Data;
using ContractManagement.API.Models;

namespace ContractManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServiceRequestsController(AppDbContext context)
        {
            _context = context;
            Console.WriteLine("=== ServiceRequestsController CREATED ===");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            Console.WriteLine("=== GET ALL SERVICE REQUESTS ===");

            var serviceRequests = await _context.ServiceRequests
                .Include(s => s.Contract)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return Ok(serviceRequests);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (serviceRequest == null)
                return NotFound();

            return Ok(serviceRequest);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceRequest serviceRequest)
        {
            var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);
            if (contract == null)
                return BadRequest(new { Message = "Contract not found" });

            if (contract.Status != "Approved")
                return BadRequest(new { Message = "Service requests can only be created against APPROVED contracts" });

            serviceRequest.CreatedAt = DateTime.UtcNow;
            serviceRequest.Status = "Pending";

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = serviceRequest.Id }, serviceRequest);
        }
    }
}