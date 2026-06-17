using Microsoft.EntityFrameworkCore;
using ContractManagement.API.Models;

namespace ContractManagement.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }  // Changed from ServiceRequests to ServiceRequest

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision
            modelBuilder.Entity<Contract>()
                .Property(c => c.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ServiceRequest>()
                .Property(s => s.AmountUSD)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ServiceRequest>()
                .Property(s => s.AmountZAR)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ServiceRequest>()
                .Property(s => s.ExchangeRateUsed)
                .HasPrecision(18, 4);

            // Seed sample data for Contracts
            modelBuilder.Entity<Contract>().HasData(
                new Contract
                {
                    Id = 1,
                    ClientName = "Acme Corporation",
                    Description = "Website Development Project",
                    Amount = 15000,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Contract
                {
                    Id = 2,
                    ClientName = "TechStart Solutions",
                    Description = "Mobile App Development",
                    Amount = 25000,
                    Status = "Approved",
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Contract
                {
                    Id = 3,
                    ClientName = "Global Finance Inc",
                    Description = "Security Audit",
                    Amount = 8000,
                    Status = "Declined",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                }
            );

            // Seed sample Service Requests
            modelBuilder.Entity<ServiceRequest>().HasData(
                new ServiceRequest
                {
                    Id = 1,
                    ContractId = 2,
                    Description = "Initial delivery of mobile app",
                    AmountUSD = 5000,
                    AmountZAR = 97500,
                    ExchangeRateUsed = 19.50m,
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    CompletedAt = DateTime.UtcNow.AddDays(-5)
                },
                new ServiceRequest
                {
                    Id = 2,
                    ContractId = 2,
                    Description = "Second phase: UI improvements",
                    AmountUSD = 3000,
                    AmountZAR = 58500,
                    ExchangeRateUsed = 19.50m,
                    Status = "InProgress",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    CompletedAt = null
                }
            );
        }
    }
}