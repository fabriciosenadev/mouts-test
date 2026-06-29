using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;

public class ListSalesRequestValidator : AbstractValidator<ListSalesRequest>
{
    public ListSalesRequestValidator()
    {
        RuleFor(query => query.Page).GreaterThan(0);
        RuleFor(query => query.Size).InclusiveBetween(1, 100);
        RuleFor(query => query)
            .Must(query => !query.StartDate.HasValue || !query.EndDate.HasValue || query.StartDate <= query.EndDate)
            .WithMessage("Start date must be less than or equal to end date");
    }
}
