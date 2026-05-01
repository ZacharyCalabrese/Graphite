using DatasetBuilder.Api.Models;

namespace DatasetBuilder.Api.Repositories;

public interface IDatasetDefinitionRepository
{
    Task<DatasetDefinition> AddAsync(DatasetDefinition entity, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DatasetDefinition>> GetAllAsync(CancellationToken cancellationToken);
    Task<DatasetDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
