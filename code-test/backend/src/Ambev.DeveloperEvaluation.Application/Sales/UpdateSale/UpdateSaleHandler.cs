using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateSaleHandler> _logger;
    private readonly ISaleRepository _saleRepository;

    public UpdateSaleHandler(IMapper mapper, ISaleRepository saleRepository, ILogger<UpdateSaleHandler> logger)
    {
        _mapper = mapper;
        _logger = logger;
        _saleRepository = saleRepository;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (sale is null)
        {
            throw new KeyNotFoundException($"Sale with ID {request.Id} not found");
        }

        if (!string.Equals(sale.SaleNumber, request.SaleNumber, StringComparison.Ordinal))
        {
            sale.SaleNumber = request.SaleNumber;
        }

        var existingItems = sale.Items.ToDictionary(item => item.Id);
        var requestedExistingItemIds = request.Items
            .Where(item => item.Id.HasValue)
            .Select(item => item.Id!.Value)
            .ToHashSet();

        var itemsToRemove = sale.Items
            .Where(item => !requestedExistingItemIds.Contains(item.Id))
            .ToList();

        foreach (var itemToRemove in itemsToRemove)
        {
            sale.Items.Remove(itemToRemove);
        }

        foreach (var requestItem in request.Items)
        {
            if (requestItem.Id.HasValue && existingItems.TryGetValue(requestItem.Id.Value, out var existingItem))
            {
                existingItem.ProductId = requestItem.ProductId;
                existingItem.Update(requestItem.ProductName, requestItem.Quantity, requestItem.UnitPrice);
                continue;
            }

            sale.Items.Add(new SaleItem
            {
                Id = requestItem.Id ?? Guid.NewGuid(),
                SaleId = sale.Id,
                ProductId = requestItem.ProductId,
                ProductName = requestItem.ProductName,
                Quantity = requestItem.Quantity,
                UnitPrice = requestItem.UnitPrice
            });
        }

        sale.Update(
            request.SaleNumber,
            request.SaleDate,
            request.CustomerId,
            request.CustomerName,
            request.BranchId,
            request.BranchName);

        var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);
        _logger.LogInformation("SaleModified {@SaleId} {@SaleNumber} {@CustomerId} {@TotalAmount}", updatedSale.Id, updatedSale.SaleNumber, updatedSale.CustomerId, updatedSale.TotalAmount);
        return _mapper.Map<UpdateSaleResult>(updatedSale);
    }
}
