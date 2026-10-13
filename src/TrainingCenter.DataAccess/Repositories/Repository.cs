namespace TrainingCenter.DataAccess.Repositories;

using Microsoft.EntityFrameworkCore;
using TrainingCenter.Core.Storage;

/// <summary>
/// Generic-репозиторий EF Core, подготавливающий изменения без вызова SaveChanges.
/// </summary>
/// <typeparam name="TEntity">Тип сущности хранилища.</typeparam>
public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    private readonly DbSet<TEntity> entities;

    /// <summary>
    /// ctor.
    /// </summary>
    /// <param name="dbContext">Общий контекст текущей единицы работы.</param>
    public Repository(TrainingCenterDbContext dbContext)
    {
        this.entities = dbContext.Set<TEntity>();
    }

    /// <inheritdoc />
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await this.entities.AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(TEntity entity)
    {
        this.entities.Update(entity);
    }
}