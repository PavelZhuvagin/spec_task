namespace TrainingCenter.Core.Models;

/// <summary>
/// Курс с ценой и временем старта.
/// </summary>
public class CourseDto
{
    /// <summary>
    /// Идентификатор курса.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Название курса.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Стоимость курса до применения скидок.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Момент старта курса с часовым смещением.
    /// </summary>
    public DateTimeOffset StartsAt { get; set; }
}