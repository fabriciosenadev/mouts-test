using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateSaleHandlerTests
{
    private readonly IMapper _mapper;
    private readonly ISaleRepository _saleRepository;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _mapper = Substitute.For<IMapper>();
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new CreateSaleHandler(_mapper, _saleRepository);
    }

    [Fact(DisplayName = "Given valid sale data When creating sale Then returns success response")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = command.SaleNumber,
            SaleDate = command.SaleDate,
            CustomerId = command.CustomerId,
            CustomerName = command.CustomerName,
            BranchId = command.BranchId,
            BranchName = command.BranchName,
            Items =
            [
                new SaleItem
                {
                    ProductId = command.Items[0].ProductId,
                    ProductName = command.Items[0].ProductName,
                    Quantity = command.Items[0].Quantity,
                    UnitPrice = command.Items[0].UnitPrice
                },
                new SaleItem
                {
                    ProductId = command.Items[1].ProductId,
                    ProductName = command.Items[1].ProductName,
                    Quantity = command.Items[1].Quantity,
                    UnitPrice = command.Items[1].UnitPrice
                }
            ]
        };

        sale.RecalculateTotals();

        var result = new CreateSaleResult
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            TotalAmount = sale.TotalAmount
        };

        _mapper.Map<Sale>(command).Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(result);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);

        var createSaleResult = await _handler.Handle(command, CancellationToken.None);

        createSaleResult.Should().NotBeNull();
        createSaleResult.Id.Should().Be(sale.Id);
        createSaleResult.TotalAmount.Should().Be(66m);
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given sale item with four units When creating sale Then applies ten percent discount")]
    public async Task Handle_FourUnits_AppliesTenPercentDiscount()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        var sale = new Sale
        {
            Items =
            [
                new SaleItem
                {
                    ProductId = command.Items[0].ProductId,
                    ProductName = command.Items[0].ProductName,
                    Quantity = 4,
                    UnitPrice = 10m
                }
            ]
        };

        _mapper.Map<Sale>(command).Returns(sale);
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult());
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);

        await _handler.Handle(command, CancellationToken.None);

        sale.Items[0].DiscountPercentage.Should().Be(0.10m);
        sale.Items[0].DiscountAmount.Should().Be(4m);
        sale.Items[0].TotalAmount.Should().Be(36m);
    }

    [Fact(DisplayName = "Given sale item above maximum quantity When creating sale Then throws validation exception")]
    public async Task Handle_QuantityAboveLimit_ThrowsValidationException()
    {
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        command.Items[0].Quantity = 21;

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
