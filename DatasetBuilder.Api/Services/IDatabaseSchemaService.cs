namespace DatasetBuilder.Api.Services;

public interface IDatabaseSchemaService
{
    Task<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> GetSchemaAsync(CancellationToken cancellationToken);
}
