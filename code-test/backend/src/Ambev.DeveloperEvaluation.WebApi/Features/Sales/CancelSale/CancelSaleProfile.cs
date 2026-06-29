using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSale;

public class CancelSaleProfile : Profile
{
    public CancelSaleProfile()
    {
        CreateMap<Guid, CancelSaleCommand>()
            .ConstructUsing(id => new CancelSaleCommand(id));

        CreateMap<CancelSaleResult, CancelSaleResponse>();
        CreateMap<CancelSaleItemResult, CancelSaleLineResponse>();
    }
}
