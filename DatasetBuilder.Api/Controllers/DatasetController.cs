using DatasetBuilder.Api.Models;
using DatasetBuilder.Api.Repositories;
using DatasetBuilder.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DatasetBuilder.Api.Controllers;

[ApiController]
[Route("api/datasets")]
public class DatasetController(
    IDatasetDefinitionRepository repository,
    IDynamicDatasetQueryService dynamicDatasetQueryService,
    IDatabaseSchemaService schemaService) : ControllerBase
{
    private static readonly HashSet<string> SupportedOperators = ["=", ">", "<", "LIKE", ">=", "<=", "<>"];

    [HttpPost]
    public async Task<ActionResult<DatasetDefinition>> Create([FromBody] DatasetDefinitionCreateRequest request, CancellationToken cancellationToken)
    {
        var schema = await schemaService.GetSchemaAsync(cancellationToken);
        var validationErrors = ValidateCreateRequest(request, schema);
        if (validationErrors.Count > 0)
        {
            return ValidationProblem(new ValidationProblemDetails(validationErrors));
        }

        var dataset = new DatasetDefinition
        {
            Name = request.Name,
            Description = request.Description,
            SourceTable = request.SourceTable,
            SelectedColumns = request.SelectedColumns.Select(c => new SelectedColumn { ColumnName = c.ColumnName, Alias = c.Alias }).ToList(),
            FilterRules = request.FilterRules.Select(f => new FilterRule { ColumnName = f.ColumnName, Operator = f.Operator, Value = f.Value }).ToList()
        };

        var created = await repository.AddAsync(dataset, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DatasetDefinition>>> GetAll(CancellationToken cancellationToken)
        => Ok(await repository.GetAllAsync(cancellationToken));

    [HttpGet("schema")]
    public async Task<ActionResult<IReadOnlyDictionary<string, IReadOnlyCollection<string>>>> GetSchema(CancellationToken cancellationToken)
        => Ok(await schemaService.GetSchemaAsync(cancellationToken));

    [HttpGet("{id:guid}/data")]
    public async Task<ActionResult<IReadOnlyCollection<dynamic>>> GetData([FromRoute] Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var definition = await repository.GetByIdAsync(id, cancellationToken);
        if (definition is null) return NotFound();

        var runtimeFilters = HttpContext.Request.Query
            .Where(kv => kv.Key is not "page" and not "pageSize")
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToString(), StringComparer.OrdinalIgnoreCase);

        var result = await dynamicDatasetQueryService.ExecuteAsync(definition, runtimeFilters, page, pageSize, cancellationToken);
        return Ok(result);
    }

    private static Dictionary<string, string[]> ValidateCreateRequest(
        DatasetDefinitionCreateRequest request,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> schema)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        void AddError(string key, string message)
        {
            if (!errors.TryGetValue(key, out var existing))
            {
                existing = [];
                errors[key] = existing;
            }

            existing.Add(message);
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            AddError(nameof(request.Name), "Name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.SourceTable))
        {
            AddError(nameof(request.SourceTable), "SourceTable is required.");
            return errors.ToDictionary(kv => kv.Key, kv => kv.Value.Distinct().ToArray(), StringComparer.OrdinalIgnoreCase);
        }

        if (!schema.TryGetValue(request.SourceTable, out var tableColumns))
        {
            AddError(nameof(request.SourceTable), $"Source table '{request.SourceTable}' was not found.");
            return errors.ToDictionary(kv => kv.Key, kv => kv.Value.Distinct().ToArray(), StringComparer.OrdinalIgnoreCase);
        }

        if (request.SelectedColumns.Count == 0)
        {
            AddError(nameof(request.SelectedColumns), "At least one selected column is required.");
        }

        var selectedColumnNames = request.SelectedColumns
            .Select(c => c.ColumnName)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var selectedColumn in request.SelectedColumns)
        {
            if (string.IsNullOrWhiteSpace(selectedColumn.ColumnName))
            {
                AddError(nameof(request.SelectedColumns), "Selected columns cannot be empty.");
                continue;
            }

            if (!tableColumns.Contains(selectedColumn.ColumnName, StringComparer.OrdinalIgnoreCase))
            {
                AddError(nameof(request.SelectedColumns), $"Column '{selectedColumn.ColumnName}' does not exist on table '{request.SourceTable}'.");
            }
        }

        foreach (var filter in request.FilterRules)
        {
            if (string.IsNullOrWhiteSpace(filter.ColumnName))
            {
                AddError(nameof(request.FilterRules), "Filter column is required.");
                continue;
            }

            if (!selectedColumnNames.Contains(filter.ColumnName))
            {
                AddError(nameof(request.FilterRules), $"Filter column '{filter.ColumnName}' must also be selected.");
            }

            if (!SupportedOperators.Contains(filter.Operator.ToUpperInvariant()))
            {
                AddError(nameof(request.FilterRules), $"Filter operator '{filter.Operator}' is not supported.");
            }
        }

        return errors.ToDictionary(kv => kv.Key, kv => kv.Value.Distinct().ToArray(), StringComparer.OrdinalIgnoreCase);
    }
}
