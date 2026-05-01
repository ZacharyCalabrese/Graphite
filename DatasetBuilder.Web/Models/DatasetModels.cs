namespace DatasetBuilder.Web.Models;

public class DatasetBuilderState
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceTable { get; set; } = string.Empty;
    public List<SelectedColumnVm> SelectedColumns { get; set; } = [];
    public List<FilterRuleVm> FilterRules { get; set; } = [];
    public Guid? PublishedId { get; set; }
}

public record SelectedColumnVm(string TableName, string ColumnName, bool Included);
public record FilterRuleVm(string ColumnName, string Operator, string Value);
