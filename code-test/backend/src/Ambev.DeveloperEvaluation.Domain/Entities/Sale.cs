using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

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
    public bool Cancelled { get; private set; }
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

    public void Update(
        string saleNumber,
        DateTime saleDate,
        Guid customerId,
        string customerName,
        Guid branchId,
        string branchName)
    {
        EnsureNotCancelled();

        SaleNumber = saleNumber;
        SaleDate = saleDate;
        CustomerId = customerId;
        CustomerName = customerName;
        BranchId = branchId;
        BranchName = branchName;

        RecalculateTotals();
    }

    public void Cancel()
    {
        if (Cancelled)
        {
            return;
        }

        Cancelled = true;

        foreach (var item in Items)
        {
            item.Cancel();
        }

        RecalculateTotals();
    }

    public void CancelItem(Guid itemId)
    {
        EnsureNotCancelled();

        var item = Items.FirstOrDefault(currentItem => currentItem.Id == itemId);
        if (item is null)
        {
            throw new KeyNotFoundException($"Sale item with ID {itemId} not found");
        }

        item.Cancel();
        RecalculateTotals();
    }

    private void EnsureNotCancelled()
    {
        if (Cancelled)
        {
            throw new DomainException("Cancelled sales cannot be changed");
        }
    }
}
