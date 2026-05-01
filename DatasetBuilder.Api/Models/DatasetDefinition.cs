namespace DatasetBuilder.Api.Models;

public class DatasetDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceTable { get; set; } = string.Empty;
    public List<SelectedColumn> SelectedColumns { get; set; } = [];
    public List<FilterRule> FilterRules { get; set; } = [];
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

public class SelectedColumn
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DatasetDefinitionId { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string? Alias { get; set; }
}

public class FilterRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DatasetDefinitionId { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string Operator { get; set; } = "=";
    public string Value { get; set; } = string.Empty;
}

public record DatasetDefinitionCreateRequest(
    string Name,
    string Description,
    string SourceTable,
    IReadOnlyCollection<SelectedColumnRequest> SelectedColumns,
    IReadOnlyCollection<FilterRuleRequest> FilterRules);

public record SelectedColumnRequest(string ColumnName, string? Alias);
public record FilterRuleRequest(string ColumnName, string Operator, string Value);
