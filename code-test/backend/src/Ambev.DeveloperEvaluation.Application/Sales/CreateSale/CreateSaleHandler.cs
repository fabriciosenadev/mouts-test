using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSaleHandler> _logger;
    private readonly ISaleRepository _saleRepository;

    public CreateSaleHandler(IMapper mapper, ISaleRepository saleRepository, ILogger<CreateSaleHandler> logger)
    {
        _mapper = mapper;
        _logger = logger;
        _saleRepository = saleRepository;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var sale = _mapper.Map<Sale>(command);
        sale.RecalculateTotals();

        var createdSale = await _saleRepository.CreateAsync(sale, cancellationToken);
        _logger.LogInformation("SaleCreated {@SaleId} {@SaleNumber} {@CustomerId} {@TotalAmount}", createdSale.Id, createdSale.SaleNumber, createdSale.CustomerId, createdSale.TotalAmount);
        return _mapper.Map<CreateSaleResult>(createdSale);
    }
}
