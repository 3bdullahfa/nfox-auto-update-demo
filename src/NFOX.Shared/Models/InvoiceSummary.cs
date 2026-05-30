namespace NFOX.Shared.Models;

public sealed class InvoiceSummary
{
    public int InvoiceCount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? LatestInvoiceDate { get; set; }
}
