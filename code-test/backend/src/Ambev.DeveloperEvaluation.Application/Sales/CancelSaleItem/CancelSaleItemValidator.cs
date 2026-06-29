using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemValidator : AbstractValidator<CancelSaleItemCommand>
{
    public CancelSaleItemValidator()
    {
        RuleFor(command => command.SaleId).NotEmpty().WithMessage("Sale ID is required");
        RuleFor(command => command.ItemId).NotEmpty().WithMessage("Sale item ID is required");
    }
}
