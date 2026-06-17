namespace ContractManagement.API.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal AmountUSD { get; set; }
        public decimal AmountZAR { get; set; }
        public decimal ExchangeRateUsed { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation property
        public Contract? Contract { get; set; }
    }
}