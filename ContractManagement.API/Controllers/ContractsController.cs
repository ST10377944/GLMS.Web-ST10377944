using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractManagement.API.Data;
using ContractManagement.API.Models;

namespace ContractManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // All endpoints require authentication
    public class ContractsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContractsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/contracts
        // GET: api/contracts?clientName=Acme&status=Pending
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? clientName, [FromQuery] string? status)
        {
            var query = _context.Contracts.AsQueryable();

            if (!string.IsNullOrEmpty(clientName))
                query = query.Where(c => c.ClientName.Contains(clientName));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            var contracts = await query.ToListAsync();
            return Ok(contracts);
        }

        // GET: api/contracts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound(new { Message = $"Contract with ID {id} not found" });

            return Ok(contract);
        }

        // POST: api/contracts
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Contract contract)
        {
            if (string.IsNullOrWhiteSpace(contract.ClientName))
                return BadRequest(new { Message = "Client name is required" });

            contract.Status = "Pending";
            contract.CreatedAt = DateTime.UtcNow;

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
        }

        // PATCH: api/contracts/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound(new { Message = $"Contract with ID {id} not found" });

            // Validate status
            if (newStatus != "Pending" && newStatus != "Approved" && newStatus != "Declined")
                return BadRequest(new { Message = "Status must be Pending, Approved, or Declined" });

            contract.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(contract);
        }

        // DELETE: api/contracts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound(new { Message = $"Contract with ID {id} not found" });

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}