namespace TrainingCenter.Core.Storage;

/// <summary>
/// Добавление и обновление сущностей без самостоятельного сохранения изменений.
/// </summary>
/// <typeparam name="TEntity">Тип сущности хранилища.</typeparam>
public interface IRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Добавляет сущность в единицу работы без сохранения в хранилище.
    /// </summary>
    /// <param name="entity">Новая сущность.</param>
    /// <param name="cancellationToken">Токен отмены добавления.</param>
    /// <returns>Задача добавления сущности в контекст.</returns>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Помечает сущность для обновления при следующем сохранении единицы работы.
    /// </summary>
    /// <param name="entity">Сущность с изменёнными значениями.</param>
    void Update(TEntity entity);
}