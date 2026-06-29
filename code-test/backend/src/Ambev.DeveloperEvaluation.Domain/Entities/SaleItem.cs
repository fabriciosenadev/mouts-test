using Ambev.DeveloperEvaluation.Domain.Common;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool Cancelled { get; set; }
    public Sale? Sale { get; set; }

    public void ApplyPricingRules()
    {
        if (Quantity > 20)
        {
            throw new ValidationException("It's not possible to sell above 20 identical items");
        }

        if (Quantity < 0)
        {
            throw new ValidationException("Quantity must be greater than zero");
        }

        DiscountPercentage = Quantity switch
        {
            >= 10 => 0.20m,
            >= 4 => 0.10m,
            _ => 0m
        };

        var grossAmount = Quantity * UnitPrice;
        DiscountAmount = Math.Round(grossAmount * DiscountPercentage, 2, MidpointRounding.AwayFromZero);
        TotalAmount = grossAmount - DiscountAmount;
    }
}
