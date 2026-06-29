using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public bool Cancelled { get; set; }
    public List<SaleItem> Items { get; set; } = [];

    public void RecalculateTotals()
    {
        foreach (var item in Items)
        {
            item.ApplyPricingRules();
        }

        TotalAmount = Items
            .Where(item => !item.Cancelled)
            .Sum(item => item.TotalAmount);
    }
}
