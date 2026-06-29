using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesHandler : IRequestHandler<ListSalesCommand, ListSalesResult>
{
    private readonly IMapper _mapper;
    private readonly ISaleRepository _saleRepository;

    public ListSalesHandler(IMapper mapper, ISaleRepository saleRepository)
    {
        _mapper = mapper;
        _saleRepository = saleRepository;
    }

    public async Task<ListSalesResult> Handle(ListSalesCommand request, CancellationToken cancellationToken)
    {
        var validator = new ListSalesValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var filter = _mapper.Map<SaleFilter>(request);
        var (items, totalCount) = await _saleRepository.SearchAsync(filter, cancellationToken);

        return new ListSalesResult
        {
            CurrentPage = request.Page,
            PageSize = request.Size,
            TotalCount = totalCount,
            Items = _mapper.Map<List<ListSalesItemResult>>(items)
        };
    }
}
