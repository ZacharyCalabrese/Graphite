using System.Data.SqlClient;
using Dapper;

namespace DatasetBuilder.Api.Services;

public class SqlServerSchemaService(IConfiguration config) : IDatabaseSchemaService
{
    public async Task<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> GetSchemaAsync(CancellationToken cancellationToken)
    {
        await using var conn = new SqlConnection(config.GetConnectionString("FixedIncomeDb"));
        const string sql = @"
SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS TableName, COLUMN_NAME AS ColumnName
FROM INFORMATION_SCHEMA.COLUMNS
ORDER BY TableName, ORDINAL_POSITION";
        var rows = await conn.QueryAsync<(string TableName, string ColumnName)>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.GroupBy(x => x.TableName).ToDictionary(g => g.Key, g => (IReadOnlyCollection<string>)g.Select(x => x.ColumnName).ToList());
    }
}
