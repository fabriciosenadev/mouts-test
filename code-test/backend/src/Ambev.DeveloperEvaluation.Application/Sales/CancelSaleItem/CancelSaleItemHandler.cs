using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    private readonly IMapper _mapper;
    private readonly ILogger<CancelSaleItemHandler> _logger;
    private readonly ISaleRepository _saleRepository;

    public CancelSaleItemHandler(IMapper mapper, ISaleRepository saleRepository, ILogger<CancelSaleItemHandler> logger)
    {
        _mapper = mapper;
        _logger = logger;
        _saleRepository = saleRepository;
    }

    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleItemValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale is null)
        {
            throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found");
        }

        sale.CancelItem(request.ItemId);

        var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);
        _logger.LogInformation("ItemCancelled {@SaleId} {@ItemId} {@SaleNumber} {@TotalAmount}", updatedSale.Id, request.ItemId, updatedSale.SaleNumber, updatedSale.TotalAmount);

        return _mapper.Map<CancelSaleItemResult>(updatedSale);
    }
}
