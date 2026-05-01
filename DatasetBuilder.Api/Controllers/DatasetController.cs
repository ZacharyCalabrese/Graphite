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
    [HttpPost]
    public async Task<ActionResult<DatasetDefinition>> Create([FromBody] DatasetDefinitionCreateRequest request, CancellationToken cancellationToken)
    {
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
}
