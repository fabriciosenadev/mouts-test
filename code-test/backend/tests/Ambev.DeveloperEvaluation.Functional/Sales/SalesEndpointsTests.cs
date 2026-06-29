using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Functional.Infrastructure;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Sales;

[CollectionDefinition(nameof(SalesApiCollection))]
public class SalesApiCollection : ICollectionFixture<SalesApiFactory>;

[Collection(nameof(SalesApiCollection))]
public class SalesEndpointsTests
{
    private readonly SalesApiFactory _factory;

    public SalesEndpointsTests(SalesApiFactory factory)
    {
        _factory = factory;
    }

    [Fact(DisplayName = "Sales endpoints should support end to end flow")]
    public async Task SalesEndpoints_ShouldSupportEndToEndFlow()
    {
        await _factory.ResetAsync();
        using var client = _factory.CreateClient();

        var createRequest = new
        {
            saleNumber = "SALE-FUNC-001",
            saleDate = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "Functional Customer",
            branchId = Guid.NewGuid(),
            branchName = "Functional Branch",
            items = new object[]
            {
                new
                {
                    productId = Guid.NewGuid(),
                    productName = "Product A",
                    quantity = 4,
                    unitPrice = 10m
                },
                new
                {
                    productId = Guid.NewGuid(),
                    productName = "Product B",
                    quantity = 2,
                    unitPrice = 15m
                }
            }
        };

        var createResponse = await client.PostAsJsonAsync("/api/sales", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using var createJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var saleId = GetPropertyCaseInsensitive(GetPropertyCaseInsensitive(createJson.RootElement, "data"), "id").GetGuid();

        var getResponse = await client.GetAsync($"/api/sales/{saleId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        using var getJson = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        var getData = GetPropertyCaseInsensitive(getJson.RootElement, "data");
        Assert.Equal("SALE-FUNC-001", GetPropertyCaseInsensitive(getData, "saleNumber").GetString());

        var listResponse = await client.GetAsync("/api/sales?page=1&size=10&customerName=Functional");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        using var listJson = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        Assert.Equal(1, GetPropertyCaseInsensitive(listJson.RootElement, "totalCount").GetInt32());

        var items = GetPropertyCaseInsensitive(getData, "items");
        var firstItemId = items[0]
            .GetProperty("id")
            .GetGuid();

        var secondItemId = items[1]
            .GetProperty("id")
            .GetGuid();

        var updateRequest = new
        {
            saleNumber = "SALE-FUNC-001-UPDATED",
            saleDate = DateTime.UtcNow,
            customerId = Guid.NewGuid(),
            customerName = "Functional Customer Updated",
            branchId = Guid.NewGuid(),
            branchName = "Functional Branch Updated",
            items = new object[]
            {
                new
                {
                    id = firstItemId,
                    productId = Guid.NewGuid(),
                    productName = "Product A Updated",
                    quantity = 4,
                    unitPrice = 10m
                },
                new
                {
                    id = secondItemId,
                    productId = Guid.NewGuid(),
                    productName = "Product B Updated",
                    quantity = 2,
                    unitPrice = 15m
                }
            }
        };

        var updateResponse = await client.PutAsJsonAsync($"/api/sales/{saleId}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var cancelItemResponse = await client.PatchAsync($"/api/sales/{saleId}/items/{secondItemId}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancelItemResponse.StatusCode);

        using var cancelItemJson = JsonDocument.Parse(await cancelItemResponse.Content.ReadAsStringAsync());
        Assert.Equal(36m, GetPropertyCaseInsensitive(GetPropertyCaseInsensitive(cancelItemJson.RootElement, "data"), "totalAmount").GetDecimal());

        var cancelSaleResponse = await client.PatchAsync($"/api/sales/{saleId}/cancel", null);
        Assert.Equal(HttpStatusCode.OK, cancelSaleResponse.StatusCode);

        using var cancelSaleJson = JsonDocument.Parse(await cancelSaleResponse.Content.ReadAsStringAsync());
        var cancelSaleData = GetPropertyCaseInsensitive(cancelSaleJson.RootElement, "data");
        Assert.True(GetPropertyCaseInsensitive(cancelSaleData, "cancelled").GetBoolean());
        Assert.Equal(0m, GetPropertyCaseInsensitive(cancelSaleData, "totalAmount").GetDecimal());
    }

    private static JsonElement GetPropertyCaseInsensitive(JsonElement element, string propertyName)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }
        }

        throw new KeyNotFoundException($"Property '{propertyName}' was not found.");
    }
}
