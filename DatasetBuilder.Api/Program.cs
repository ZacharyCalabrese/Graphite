using DatasetBuilder.Api.Data;
using DatasetBuilder.Api.Repositories;
using DatasetBuilder.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DatasetConfigDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConfigDb")));

builder.Services.AddScoped<IDatasetDefinitionRepository, DatasetDefinitionRepository>();
builder.Services.AddScoped<IDynamicDatasetQueryService, DynamicDatasetQueryService>();
builder.Services.AddScoped<IDatabaseSchemaService, SqlServerSchemaService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
