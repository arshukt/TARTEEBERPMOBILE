using TarteebErp.Shared.Enums;

namespace TarteebErp.Domain.Entities;

public class Payment : BaseEntity
{
    public DateTime PaymentDate { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public PaymentType PaymentType { get; set; }
    public PartyType PartyType { get; set; }
    public int PartyId { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
