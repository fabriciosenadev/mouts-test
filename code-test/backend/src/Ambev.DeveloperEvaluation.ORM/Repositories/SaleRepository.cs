using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(sale => sale.Items)
            .FirstOrDefaultAsync(sale => sale.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<Sale> Items, int TotalCount)> SearchAsync(SaleFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Sales
            .Include(sale => sale.Items)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SaleNumber))
        {
            query = query.Where(sale => sale.SaleNumber.Contains(filter.SaleNumber));
        }

        if (!string.IsNullOrWhiteSpace(filter.CustomerName))
        {
            query = query.Where(sale => sale.CustomerName.Contains(filter.CustomerName));
        }

        if (!string.IsNullOrWhiteSpace(filter.BranchName))
        {
            query = query.Where(sale => sale.BranchName.Contains(filter.BranchName));
        }

        if (filter.Cancelled.HasValue)
        {
            query = query.Where(sale => sale.Cancelled == filter.Cancelled.Value);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(sale => sale.SaleDate >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(sale => sale.SaleDate <= filter.EndDate.Value);
        }

        query = query.OrderByDescending(sale => sale.SaleDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filter.Page - 1) * filter.Size)
            .Take(filter.Size)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }
}
