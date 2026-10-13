namespace TrainingCenter.DataAccess;

using TrainingCenter.Core.Storage;
using TrainingCenter.DataAccess.Repositories;

/// <summary>
/// Единица работы с репозиториями на одном scoped-контексте EF Core.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly TrainingCenterDbContext dbContext;
    private readonly Dictionary<Type, object> repositories = new();

    /// <summary>
    /// ctor.
    /// </summary>
    /// <param name="dbContext">Контекст хранилища, жизненным циклом которого управляет DI.</param>
    public UnitOfWork(TrainingCenterDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public IRepository<TEntity> GetRepository<TEntity>()
        where TEntity : class
    {
        var entityType = typeof(TEntity);
        if (!this.repositories.TryGetValue(entityType, out var repository))
        {
            repository = new Repository<TEntity>(this.dbContext);
            this.repositories.Add(entityType, repository);
        }

        return (IRepository<TEntity>)repository;
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return this.dbContext.SaveChangesAsync(cancellationToken);
    }
}