namespace TrainingCenter.Core.Storage;

/// <summary>
/// Координирует репозитории на общем контексте и сохраняет накопленные изменения.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Возвращает репозиторий сущности, связанный с текущей единицей работы.
    /// </summary>
    /// <typeparam name="TEntity">Тип сущности хранилища.</typeparam>
    /// <returns>Один и тот же экземпляр репозитория для данного типа в пределах единицы работы.</returns>
    IRepository<TEntity> GetRepository<TEntity>()
        where TEntity : class;

    /// <summary>
    /// Сохраняет изменения всех репозиториев текущей единицы работы.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены сохранения.</param>
    /// <returns>Количество записей состояния, сохранённых контекстом.</returns>
    /// <remarks>
    /// EF Core InMemory не поддерживает транзакции и rollback; единица работы объединяет отслеживание и сохранение.
    /// </remarks>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}