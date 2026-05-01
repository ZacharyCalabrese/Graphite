using System.Data.SqlClient;
using Dapper;
using DatasetBuilder.Api.Models;
using SqlKata;
using SqlKata.Compilers;

namespace DatasetBuilder.Api.Services;

public class DynamicDatasetQueryService(IConfiguration configuration) : IDynamicDatasetQueryService
{
    private static readonly HashSet<string> AllowedOperators = ["=", ">", "<", "LIKE", ">=", "<=", "<>"];

    public async Task<IReadOnlyCollection<dynamic>> ExecuteAsync(
        DatasetDefinition definition,
        IReadOnlyDictionary<string, string> runtimeFilters,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        // SECURITY: SqlKata quote-wraps identifiers when compiling SQL Server queries.
        // We additionally validate input to allow only known-safe column names from the saved dataset definition.
        var allowedColumns = definition.SelectedColumns.Select(c => c.ColumnName).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var query = new Query(definition.SourceTable);

        foreach (var column in definition.SelectedColumns)
        {
            query.Select(string.IsNullOrWhiteSpace(column.Alias)
                ? column.ColumnName
                : $"{column.ColumnName} as {column.Alias}");
        }

        foreach (var filter in definition.FilterRules)
        {
            ApplyFilter(query, allowedColumns, filter.ColumnName, filter.Operator, filter.Value);
        }

        // SECURITY: Runtime filter keys are user input from query string.
        // We only accept keys that match the persisted selected columns and bind values as SQL parameters.
        foreach (var runtimeFilter in runtimeFilters)
        {
            ApplyFilter(query, allowedColumns, runtimeFilter.Key, "=", runtimeFilter.Value);
        }

        query.Offset(Math.Max(0, (page - 1) * pageSize)).Limit(Math.Clamp(pageSize, 1, 500));

        var compiler = new SqlServerCompiler();
        var compiled = compiler.Compile(query);

        await using var connection = new SqlConnection(configuration.GetConnectionString("FixedIncomeDb"));
        var command = new CommandDefinition(compiled.Sql, compiled.NamedBindings, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync(command);
        return rows.ToList();
    }

    private static void ApplyFilter(Query query, HashSet<string> allowedColumns, string column, string @operator, object value)
    {
        if (!allowedColumns.Contains(column))
        {
            throw new InvalidOperationException($"Column '{column}' is not allowed for filtering.");
        }

        if (!AllowedOperators.Contains(@operator.ToUpperInvariant()))
        {
            throw new InvalidOperationException($"Operator '{@operator}' is not supported.");
        }

        query.Where(column, @operator, value);
    }
}
