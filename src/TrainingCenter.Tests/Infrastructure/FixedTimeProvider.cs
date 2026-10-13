namespace TrainingCenter.Tests.Infrastructure;

/// <summary>
/// Фиксированный источник времени для воспроизводимых расчётов скидок.
/// </summary>
internal class FixedTimeProvider : TimeProvider
{
    /// <summary>
    /// Возвращает фиксированный момент 8 октября 2026 года, 12:00 UTC.
    /// </summary>
    /// <returns>Момент времени, общий для тестовых курсов.</returns>
    public override DateTimeOffset GetUtcNow() => new(2026, 10, 8, 12, 0, 0, TimeSpan.Zero);
}