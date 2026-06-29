using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleHandlerTests
{
    private readonly IMapper _mapper;
    private readonly ISaleRepository _saleRepository;
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests()
    {
        _mapper = Substitute.For<IMapper>();
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new CancelSaleHandler(_mapper, _saleRepository, NullLogger<CancelSaleHandler>.Instance);
    }

    [Fact(DisplayName = "Given existing sale When cancelling Then sale and items are cancelled")]
    public async Task Handle_ExistingSale_CancelsSale()
    {
        var saleId = Guid.NewGuid();
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
                    Id = Guid.NewGuid(),
                    SaleId = saleId,
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product A",
                    Quantity = 4,
                    UnitPrice = 10m
                }
            ]
        };

        sale.RecalculateTotals();

        var result = new CancelSaleResult
        {
            Id = saleId,
            Cancelled = true,
            TotalAmount = 0m
        };

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<CancelSaleResult>(sale).Returns(result);

        var response = await _handler.Handle(new CancelSaleCommand(saleId), CancellationToken.None);

        response.Cancelled.Should().BeTrue();
        sale.Cancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(0m);
        sale.Items.Should().OnlyContain(item => item.Cancelled);
    }

    [Fact(DisplayName = "Given missing sale When cancelling Then throws key not found")]
    public async Task Handle_MissingSale_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var action = () => _handler.Handle(new CancelSaleCommand(saleId), CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }
}
