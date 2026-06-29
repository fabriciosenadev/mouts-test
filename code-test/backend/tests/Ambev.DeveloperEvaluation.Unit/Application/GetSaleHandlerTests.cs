using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class GetSaleHandlerTests
{
    private readonly IMapper _mapper;
    private readonly ISaleRepository _saleRepository;
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _mapper = Substitute.For<IMapper>();
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new GetSaleHandler(_mapper, _saleRepository);
    }

    [Fact(DisplayName = "Given existing sale When retrieving by id Then returns sale details")]
    public async Task Handle_ExistingSale_ReturnsSaleDetails()
    {
        var saleId = Guid.NewGuid();
        var command = new GetSaleCommand(saleId);
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
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product A",
                    Quantity = 4,
                    UnitPrice = 10m
                }
            ]
        };

        sale.RecalculateTotals();

        var result = new GetSaleResult
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            TotalAmount = sale.TotalAmount
        };

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(result);

        var response = await _handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.Id.Should().Be(saleId);
        response.TotalAmount.Should().Be(36m);
    }

    [Fact(DisplayName = "Given missing sale When retrieving by id Then throws key not found")]
    public async Task Handle_MissingSale_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        var command = new GetSaleCommand(saleId);

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }
}
