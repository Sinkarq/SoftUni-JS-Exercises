﻿using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace TodoApp.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    private static readonly MethodInfo? SetIsDeletedQueryFilterMethod =
        typeof(ApplicationDbContext).GetMethod(
            nameof(SetIsDeletedQueryFilter),
            BindingFlags.NonPublic | BindingFlags.Static);

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    //DbSets
    public DbSet<Todo> Todos { get; set; }

    public override int SaveChanges() => this.SaveChanges(true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.ApplyAuditInfoRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        this.SaveChangesAsync(true, cancellationToken);

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        this.ApplyAuditInfoRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Needed for Identity models configuration
        base.OnModelCreating(builder);

        this.ConfigureUserIdentityRelations(builder);

        EntityIndexesConfiguration.Configure(builder);

        var entityTypes = builder.Model.GetEntityTypes().ToList();

        // Set global query filter for not deleted entities only
        var deletableEntityTypes = entityTypes
            .Where(et => typeof(IDeletableEntity).IsAssignableFrom(et.ClrType));
        foreach (var deletableEntityType in deletableEntityTypes)
        {
            var method = SetIsDeletedQueryFilterMethod.MakeGenericMethod(deletableEntityType.ClrType);
            method.Invoke(null, new object[] {builder});
        }

        // Disable cascade delete
        var foreignKeys = entityTypes
            .SelectMany(e => e.GetForeignKeys().Where(f => f.DeleteBehavior == DeleteBehavior.Cascade));
        foreach (var foreignKey in foreignKeys)
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    private static void SetIsDeletedQueryFilter<T>(ModelBuilder builder)
        where T : class, IDeletableEntity
    {
        builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    // Applies configurations
    private void ConfigureUserIdentityRelations(ModelBuilder builder)
        => builder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);

    private void ApplyAuditInfoRules()
    {
        var changedEntries = this.ChangeTracker
            .Entries()
            .Where(e =>
                e.Entity is IAuditInfo &&
                e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in changedEntries)
        {
            var entity = (IAuditInfo) entry.Entity;
            if (entry.State == EntityState.Added && entity.CreatedOn == default)
            {
                entity.CreatedOn = DateTime.UtcNow;
            }
            else
            {
                entity.ModifiedOn = DateTime.UtcNow;
            }
        }
    }
}