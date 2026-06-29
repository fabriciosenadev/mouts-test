using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

namespace Ambev.DeveloperEvaluation.Unit.Application.TestData;

public static class CreateSaleHandlerTestData
{
    public static CreateSaleCommand GenerateValidCommand()
    {
        return new CreateSaleCommand
        {
            SaleNumber = "SALE-0001",
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer One",
            BranchId = Guid.NewGuid(),
            BranchName = "Main Branch",
            Items =
            [
                new CreateSaleItemCommand
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product A",
                    Quantity = 4,
                    UnitPrice = 10m
                },
                new CreateSaleItemCommand
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product B",
                    Quantity = 2,
                    UnitPrice = 15m
                }
            ]
        };
    }
}
