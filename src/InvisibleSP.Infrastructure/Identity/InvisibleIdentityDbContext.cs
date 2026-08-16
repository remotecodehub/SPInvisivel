namespace InvisibleSP.Infrastructure.Identity;

public sealed class InvisibleIdentityDbContext(
    DbContextOptions<InvisibleIdentityDbContext> options) : IdentityDbContext<IdentityUser, IdentityRole, string>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(property), parameter);
            builder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplySoftDelete();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override int SaveChanges()
    {
        ApplySoftDelete();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplySoftDelete();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplySoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplySoftDelete()
    {
        foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = DateTimeOffset.UtcNow;
        }
    }
}
