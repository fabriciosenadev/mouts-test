using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class UpdateSaleHandlerTests
{
    private readonly IMapper _mapper;
    private readonly ISaleRepository _saleRepository;
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _mapper = Substitute.For<IMapper>();
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new UpdateSaleHandler(_mapper, _saleRepository);
    }

    [Fact(DisplayName = "Given existing sale When updating Then recalculates totals and returns updated sale")]
    public async Task Handle_ExistingSale_UpdatesSale()
    {
        var saleId = Guid.NewGuid();
        var existingItemId = Guid.NewGuid();
        var sale = new Sale
        {
            Id = saleId,
            SaleNumber = "SALE-0001",
            SaleDate = DateTime.UtcNow.AddDays(-1),
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer One",
            BranchId = Guid.NewGuid(),
            BranchName = "Main Branch",
            Items =
            [
                new SaleItem
                {
                    Id = existingItemId,
                    SaleId = saleId,
                    ProductId = Guid.NewGuid(),
                    ProductName = "Old Product",
                    Quantity = 2,
                    UnitPrice = 10m
                }
            ]
        };

        sale.RecalculateTotals();

        var command = new UpdateSaleCommand
        {
            Id = saleId,
            SaleNumber = "SALE-0001-UPDATED",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer Two",
            BranchId = Guid.NewGuid(),
            BranchName = "North Branch",
            Items =
            [
                new UpdateSaleItemCommand
                {
                    Id = existingItemId,
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product A",
                    Quantity = 4,
                    UnitPrice = 10m
                },
                new UpdateSaleItemCommand
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product B",
                    Quantity = 2,
                    UnitPrice = 15m
                }
            ]
        };

        var result = new UpdateSaleResult
        {
            Id = saleId,
            SaleNumber = command.SaleNumber,
            TotalAmount = 66m
        };

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<UpdateSaleResult>(sale).Returns(result);

        var response = await _handler.Handle(command, CancellationToken.None);

        response.SaleNumber.Should().Be("SALE-0001-UPDATED");
        response.TotalAmount.Should().Be(66m);
        sale.Items.Should().HaveCount(2);
        sale.TotalAmount.Should().Be(66m);
    }

    [Fact(DisplayName = "Given missing sale When updating Then throws key not found")]
    public async Task Handle_MissingSale_ThrowsKeyNotFoundException()
    {
        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            SaleNumber = "SALE-0001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer One",
            BranchId = Guid.NewGuid(),
            BranchName = "Main Branch",
            Items =
            [
                new UpdateSaleItemCommand
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product A",
                    Quantity = 4,
                    UnitPrice = 10m
                }
            ]
        };

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }
}
