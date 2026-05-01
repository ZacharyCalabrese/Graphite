using DatasetBuilder.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DatasetBuilder.Api.Data;

public class DatasetConfigDbContext(DbContextOptions<DatasetConfigDbContext> options) : DbContext(options)
{
    public DbSet<DatasetDefinition> DatasetDefinitions => Set<DatasetDefinition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DatasetDefinition>()
            .HasMany(d => d.SelectedColumns)
            .WithOne()
            .HasForeignKey(c => c.DatasetDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DatasetDefinition>()
            .HasMany(d => d.FilterRules)
            .WithOne()
            .HasForeignKey(f => f.DatasetDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
