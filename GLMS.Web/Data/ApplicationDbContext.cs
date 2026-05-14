using Microsoft.EntityFrameworkCore;
using GLMS.Web.Models;

namespace GLMS.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Client>().HasData(
            new Client { ClientId = 1, Name = "TechMove SA", Email = "info@techmove.co.za", Phone = "+27 11 123 4567", Region = "Africa" },
            new Client { ClientId = 2, Name = "Global Freight Ltd", Email = "contact@globalfreight.com", Phone = "+1 212 555 1234", Region = "North America" }
        );

        modelBuilder.Entity<Contract>().HasData(
            new Contract { ContractId = 1, ClientId = 1, StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2025, 12, 31), Status = ContractStatus.Active, ServiceLevel = ServiceLevel.Premium },
            new Contract { ContractId = 2, ClientId = 2, StartDate = new DateTime(2023, 1, 1), EndDate = new DateTime(2023, 12, 31), Status = ContractStatus.Expired, ServiceLevel = ServiceLevel.Standard }
        );
    }
}