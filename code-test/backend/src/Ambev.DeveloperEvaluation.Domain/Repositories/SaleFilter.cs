namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public class SaleFilter
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? SaleNumber { get; set; }
    public string? CustomerName { get; set; }
    public string? BranchName { get; set; }
    public bool? Cancelled { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
