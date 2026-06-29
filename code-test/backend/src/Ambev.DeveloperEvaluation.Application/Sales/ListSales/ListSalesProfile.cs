using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesProfile : Profile
{
    public ListSalesProfile()
    {
        CreateMap<ListSalesCommand, SaleFilter>();
        CreateMap<Sale, ListSalesItemResult>()
            .ForMember(destination => destination.ItemCount, options => options.MapFrom(source => source.Items.Count));
    }
}
