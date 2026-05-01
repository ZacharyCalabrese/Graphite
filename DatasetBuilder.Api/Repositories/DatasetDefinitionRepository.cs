using DatasetBuilder.Api.Data;
using DatasetBuilder.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DatasetBuilder.Api.Repositories;

public class DatasetDefinitionRepository(DatasetConfigDbContext db) : IDatasetDefinitionRepository
{
    public async Task<DatasetDefinition> AddAsync(DatasetDefinition entity, CancellationToken cancellationToken)
    {
        db.DatasetDefinitions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<IReadOnlyCollection<DatasetDefinition>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.DatasetDefinitions.AsNoTracking().Include(d => d.SelectedColumns).Include(d => d.FilterRules).ToListAsync(cancellationToken);

    public async Task<DatasetDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await db.DatasetDefinitions.AsNoTracking().Include(d => d.SelectedColumns).Include(d => d.FilterRules)
            .SingleOrDefaultAsync(d => d.Id == id, cancellationToken);
}
