using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Integration.Infrastructure;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Sales;

[CollectionDefinition(nameof(PostgresCollection))]
public class PostgresCollection : ICollectionFixture<PostgresIntegrationFixture>;

[Collection(nameof(PostgresCollection))]
public class SaleRepositoryIntegrationTests
{
    private readonly PostgresIntegrationFixture _fixture;

    public SaleRepositoryIntegrationTests(PostgresIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(DisplayName = "Repository should create and retrieve sale with items")]
    public async Task CreateAndGetById_ShouldPersistSale()
    {
        await _fixture.ResetAsync();

        await using var context = new DefaultContext(_fixture.CreateOptions());
        var repository = new SaleRepository(context);

        var sale = CreateSale("SALE-INT-001", "Customer One", "Main Branch", 4, 10m);
        sale.RecalculateTotals();

        var createdSale = await repository.CreateAsync(sale, CancellationToken.None);
        var fetchedSale = await repository.GetByIdAsync(createdSale.Id, CancellationToken.None);

        Assert.NotNull(fetchedSale);
        Assert.Equal("SALE-INT-001", fetchedSale!.SaleNumber);
        Assert.Single(fetchedSale.Items);
        Assert.Equal(36m, fetchedSale.TotalAmount);
    }

    [Fact(DisplayName = "Repository should search sales using filters and pagination")]
    public async Task SearchAsync_ShouldApplyFilters()
    {
        await _fixture.ResetAsync();

        await using var context = new DefaultContext(_fixture.CreateOptions());
        var repository = new SaleRepository(context);

        var firstSale = CreateSale("SALE-INT-002", "Customer Alpha", "North Branch", 4, 10m);
        firstSale.RecalculateTotals();

        var secondSale = CreateSale("SALE-INT-003", "Customer Beta", "South Branch", 2, 15m);
        secondSale.RecalculateTotals();
        secondSale.Cancel();

        await repository.CreateAsync(firstSale, CancellationToken.None);
        await repository.CreateAsync(secondSale, CancellationToken.None);

        var filter = new SaleFilter
        {
            Page = 1,
            Size = 10,
            CustomerName = "Alpha",
            Cancelled = false
        };

        var (items, totalCount) = await repository.SearchAsync(filter, CancellationToken.None);

        Assert.Single(items);
        Assert.Equal(1, totalCount);
        Assert.Equal("SALE-INT-002", items[0].SaleNumber);
    }

    private static Sale CreateSale(string saleNumber, string customerName, string branchName, int quantity, decimal unitPrice)
    {
        return new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = saleNumber,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = customerName,
            BranchId = Guid.NewGuid(),
            BranchName = branchName,
            Items =
            [
                new SaleItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product A",
                    Quantity = quantity,
                    UnitPrice = unitPrice
                }
            ]
        };
    }
}
