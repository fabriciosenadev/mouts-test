using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;

public class CancelSaleItemRequestValidator : AbstractValidator<CancelSaleItemRequest>
{
    public CancelSaleItemRequestValidator()
    {
        RuleFor(command => command.SaleId).NotEmpty().WithMessage("Sale ID is required");
        RuleFor(command => command.ItemId).NotEmpty().WithMessage("Sale item ID is required");
    }
}
