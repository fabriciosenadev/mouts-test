using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class ListSalesHandlerTests
{
    private readonly IMapper _mapper;
    private readonly ISaleRepository _saleRepository;
    private readonly ListSalesHandler _handler;

    public ListSalesHandlerTests()
    {
        _mapper = Substitute.For<IMapper>();
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new ListSalesHandler(_mapper, _saleRepository);
    }

    [Fact(DisplayName = "Given valid filters When listing sales Then returns paginated result")]
    public async Task Handle_ValidRequest_ReturnsPaginatedResult()
    {
        var command = new ListSalesCommand
        {
            Page = 1,
            Size = 10,
            CustomerName = "Customer"
        };

        var filter = new SaleFilter
        {
            Page = command.Page,
            Size = command.Size,
            CustomerName = command.CustomerName
        };

        var sales = new List<Sale>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-0001",
                SaleDate = DateTime.UtcNow,
                CustomerId = Guid.NewGuid(),
                CustomerName = "Customer One",
                BranchId = Guid.NewGuid(),
                BranchName = "Main Branch",
                Items = []
            }
        };

        var mappedItems = new List<ListSalesItemResult>
        {
            new()
            {
                Id = sales[0].Id,
                SaleNumber = sales[0].SaleNumber,
                CustomerName = sales[0].CustomerName,
                BranchName = sales[0].BranchName
            }
        };

        _mapper.Map<SaleFilter>(command).Returns(filter);
        _saleRepository.SearchAsync(filter, Arg.Any<CancellationToken>()).Returns((sales, 1));
        _mapper.Map<List<ListSalesItemResult>>(sales).Returns(mappedItems);

        var response = await _handler.Handle(command, CancellationToken.None);

        response.Should().NotBeNull();
        response.CurrentPage.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.TotalCount.Should().Be(1);
        response.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Given invalid date range When listing sales Then throws validation exception")]
    public async Task Handle_InvalidDateRange_ThrowsValidationException()
    {
        var command = new ListSalesCommand
        {
            Page = 1,
            Size = 10,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(-1)
        };

        var action = () => _handler.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
