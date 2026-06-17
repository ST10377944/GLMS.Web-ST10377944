namespace ContractManagement.API.Models
{
    public class Contract
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Declined
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}