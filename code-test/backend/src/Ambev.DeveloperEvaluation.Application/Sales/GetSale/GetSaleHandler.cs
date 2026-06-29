using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public class GetSaleHandler : IRequestHandler<GetSaleCommand, GetSaleResult>
{
    private readonly IMapper _mapper;
    private readonly ISaleRepository _saleRepository;

    public GetSaleHandler(IMapper mapper, ISaleRepository saleRepository)
    {
        _mapper = mapper;
        _saleRepository = saleRepository;
    }

    public async Task<GetSaleResult> Handle(GetSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new GetSaleValidator();
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

        return _mapper.Map<GetSaleResult>(sale);
    }
}
