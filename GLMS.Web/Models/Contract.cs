using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Web.Models;

public enum ContractStatus
{
    Draft,
    Active,
    Expired,
    OnHold
}

public enum ServiceLevel
{
    Standard,
    Express,
    Premium
}

public class Contract
{
    [Key]
    public int ContractId { get; set; }

    [Required]
    public int ClientId { get; set; }

    [ForeignKey("ClientId")]
    public Client? Client { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Required]
    public ServiceLevel ServiceLevel { get; set; } = ServiceLevel.Standard;

    [StringLength(500)]
    public string? SignedAgreementPath { get; set; }

    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}