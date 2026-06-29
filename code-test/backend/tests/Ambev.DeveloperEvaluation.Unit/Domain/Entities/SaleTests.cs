using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using FluentValidation;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact(DisplayName = "Sale should apply zero discount for quantities below four")]
    public void Given_ItemBelowFourUnits_When_Recalculating_Then_ShouldApplyZeroDiscount()
    {
        var sale = CreateSaleWithSingleItem(quantity: 3, unitPrice: 10m);

        sale.RecalculateTotals();

        Assert.Equal(0m, sale.Items[0].DiscountPercentage);
        Assert.Equal(0m, sale.Items[0].DiscountAmount);
        Assert.Equal(30m, sale.Items[0].TotalAmount);
        Assert.Equal(30m, sale.TotalAmount);
    }

    [Fact(DisplayName = "Sale should apply ten percent discount for quantities between four and nine")]
    public void Given_ItemBetweenFourAndNineUnits_When_Recalculating_Then_ShouldApplyTenPercentDiscount()
    {
        var sale = CreateSaleWithSingleItem(quantity: 4, unitPrice: 10m);

        sale.RecalculateTotals();

        Assert.Equal(0.10m, sale.Items[0].DiscountPercentage);
        Assert.Equal(4m, sale.Items[0].DiscountAmount);
        Assert.Equal(36m, sale.Items[0].TotalAmount);
        Assert.Equal(36m, sale.TotalAmount);
    }

    [Fact(DisplayName = "Sale should apply twenty percent discount for quantities between ten and twenty")]
    public void Given_ItemBetweenTenAndTwentyUnits_When_Recalculating_Then_ShouldApplyTwentyPercentDiscount()
    {
        var sale = CreateSaleWithSingleItem(quantity: 10, unitPrice: 10m);

        sale.RecalculateTotals();

        Assert.Equal(0.20m, sale.Items[0].DiscountPercentage);
        Assert.Equal(20m, sale.Items[0].DiscountAmount);
        Assert.Equal(80m, sale.Items[0].TotalAmount);
        Assert.Equal(80m, sale.TotalAmount);
    }

    [Fact(DisplayName = "Sale should reject items above twenty units")]
    public void Given_ItemAboveTwentyUnits_When_Recalculating_Then_ShouldThrowValidationException()
    {
        var sale = CreateSaleWithSingleItem(quantity: 21, unitPrice: 10m);

        var action = () => sale.RecalculateTotals();

        Assert.Throws<ValidationException>(action);
    }

    [Fact(DisplayName = "Sale should reject items with zero quantity")]
    public void Given_ItemWithZeroQuantity_When_Recalculating_Then_ShouldThrowValidationException()
    {
        var sale = CreateSaleWithSingleItem(quantity: 0, unitPrice: 10m);

        var action = () => sale.RecalculateTotals();

        Assert.Throws<ValidationException>(action);
    }

    [Fact(DisplayName = "Sale should recalculate total ignoring cancelled items")]
    public void Given_CancelledItem_When_Recalculating_Then_TotalShouldIgnoreCancelledItem()
    {
        var activeItem = new SaleItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Product A",
            Quantity = 4,
            UnitPrice = 10m
        };

        var cancelledItem = new SaleItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Product B",
            Quantity = 2,
            UnitPrice = 15m
        };

        cancelledItem.Cancel();

        var sale = CreateSale([activeItem, cancelledItem]);

        sale.RecalculateTotals();

        Assert.Equal(36m, sale.TotalAmount);
        Assert.Equal(0m, cancelledItem.TotalAmount);
    }

    [Fact(DisplayName = "Sale cancel should cancel all items and zero total")]
    public void Given_ActiveSale_When_Cancelled_Then_ShouldCancelAllItemsAndZeroTotal()
    {
        var sale = CreateSale(
        [
            new SaleItem
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = "Product A",
                Quantity = 4,
                UnitPrice = 10m
            },
            new SaleItem
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = "Product B",
                Quantity = 2,
                UnitPrice = 15m
            }
        ]);

        sale.RecalculateTotals();
        sale.Cancel();

        Assert.True(sale.Cancelled);
        Assert.All(sale.Items, item => Assert.True(item.Cancelled));
        Assert.Equal(0m, sale.TotalAmount);
    }

    [Fact(DisplayName = "Sale cancel item should recalculate total")]
    public void Given_ActiveSale_When_ItemIsCancelled_Then_TotalShouldBeRecalculated()
    {
        var firstItem = new SaleItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Product A",
            Quantity = 4,
            UnitPrice = 10m
        };

        var secondItem = new SaleItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Product B",
            Quantity = 2,
            UnitPrice = 15m
        };

        var sale = CreateSale([firstItem, secondItem]);
        sale.RecalculateTotals();

        sale.CancelItem(secondItem.Id);

        Assert.True(secondItem.Cancelled);
        Assert.Equal(36m, sale.TotalAmount);
    }

    [Fact(DisplayName = "Cancelled sale should reject updates")]
    public void Given_CancelledSale_When_Updated_Then_ShouldThrowDomainException()
    {
        var sale = CreateSaleWithSingleItem(quantity: 4, unitPrice: 10m);
        sale.Cancel();

        var action = () => sale.Update(
            DateTime.UtcNow,
            Guid.NewGuid(),
            "Another Customer",
            Guid.NewGuid(),
            "Another Branch",
            []);

        Assert.Throws<DomainException>(action);
    }

    private static Sale CreateSaleWithSingleItem(int quantity, decimal unitPrice)
    {
        return CreateSale(
        [
            new SaleItem
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = "Product A",
                Quantity = quantity,
                UnitPrice = unitPrice
            }
        ]);
    }

    private static Sale CreateSale(List<SaleItem> items)
    {
        return new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = "SALE-0001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer One",
            BranchId = Guid.NewGuid(),
            BranchName = "Main Branch",
            Items = items
        };
    }
}
