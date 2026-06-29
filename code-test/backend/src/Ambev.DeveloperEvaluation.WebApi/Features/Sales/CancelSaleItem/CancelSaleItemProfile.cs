using AutoMapper;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;

public class CancelSaleItemProfile : Profile
{
    public CancelSaleItemProfile()
    {
        CreateMap<CancelSaleItemRequest, CancelSaleItemCommand>()
            .ConstructUsing(request => new CancelSaleItemCommand(request.SaleId, request.ItemId));

        CreateMap<CancelSaleItemResult, CancelSaleItemResponse>();
        CreateMap<CancelSaleItemItemResult, CancelSaleItemItemResponse>();
    }
}
