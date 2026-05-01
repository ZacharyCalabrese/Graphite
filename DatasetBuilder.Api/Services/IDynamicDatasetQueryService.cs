using DatasetBuilder.Api.Models;

namespace DatasetBuilder.Api.Services;

public interface IDynamicDatasetQueryService
{
    Task<IReadOnlyCollection<dynamic>> ExecuteAsync(
        DatasetDefinition definition,
        IReadOnlyDictionary<string, string> runtimeFilters,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
