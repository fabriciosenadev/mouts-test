using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleItemHandlerTests
{
    private readonly IMapper _mapper;
    private readonly ISaleRepository _saleRepository;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _mapper = Substitute.For<IMapper>();
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new CancelSaleItemHandler(_mapper, _saleRepository);
    }

    [Fact(DisplayName = "Given existing sale item When cancelling Then recalculates sale total")]
    public async Task Handle_ExistingSaleItem_CancelsItem()
    {
        var saleId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var sale = new Sale
        {
            Id = saleId,
            SaleNumber = "SALE-0001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer One",
            BranchId = Guid.NewGuid(),
            BranchName = "Main Branch",
            Items =
            [
                new SaleItem
                {
                    Id = itemId,
                    SaleId = saleId,
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product A",
                    Quantity = 4,
                    UnitPrice = 10m
                },
                new SaleItem
                {
                    Id = Guid.NewGuid(),
                    SaleId = saleId,
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product B",
                    Quantity = 2,
                    UnitPrice = 15m
                }
            ]
        };

        sale.RecalculateTotals();

        var result = new CancelSaleItemResult
        {
            Id = saleId,
            Cancelled = false,
            TotalAmount = 30m
        };

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<CancelSaleItemResult>(sale).Returns(result);

        var response = await _handler.Handle(new CancelSaleItemCommand(saleId, itemId), CancellationToken.None);

        response.TotalAmount.Should().Be(30m);
        sale.Items.First(item => item.Id == itemId).Cancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(30m);
    }

    [Fact(DisplayName = "Given missing sale When cancelling item Then throws key not found")]
    public async Task Handle_MissingSale_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var action = () => _handler.Handle(new CancelSaleItemCommand(saleId, itemId), CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }
}
