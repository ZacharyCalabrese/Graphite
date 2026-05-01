using System.Net.Http.Json;
using DatasetBuilder.Web.Models;

namespace DatasetBuilder.Web.Services;

public class DatasetApiClient(HttpClient http)
{
    public async Task<Dictionary<string, IReadOnlyCollection<string>>> GetSchemaAsync() =>
        await http.GetFromJsonAsync<Dictionary<string, IReadOnlyCollection<string>>>("api/datasets/schema") ?? [];

    public async Task<IReadOnlyCollection<Dictionary<string, object>>> PreviewAsync(Guid datasetId, int page = 1, int pageSize = 50)
        => await http.GetFromJsonAsync<IReadOnlyCollection<Dictionary<string, object>>>($"api/datasets/{datasetId}/data?page={page}&pageSize={pageSize}") ?? [];

    public async Task<Guid> PublishAsync(DatasetBuilderState state)
    {
        var payload = new
        {
            state.Name,
            state.Description,
            SourceTable = state.SourceTable,
            SelectedColumns = state.SelectedColumns.Where(c => c.Included).Select(c => new { ColumnName = c.ColumnName, Alias = (string?)null }),
            FilterRules = state.FilterRules
        };

        var response = await http.PostAsJsonAsync("api/datasets", payload);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        return Guid.Parse(created!["id"].ToString()!);
    }
}
