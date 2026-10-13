namespace TrainingCenter.BusinessLogic.Models.Cart;

/// <summary>
/// Результат расчёта стоимости отдельного курса.
/// </summary>
public class CourseTotalDto
{
    /// <summary>
    /// Идентификатор курса.
    /// </summary>
    public Guid CourseId { get; set; }

    /// <summary>
    /// Стоимость до применения скидки.
    /// </summary>
    public decimal OriginalPrice { get; set; }

    /// <summary>
    /// Максимальная доступная скидка в процентах.
    /// </summary>
    public int DiscountPercent { get; set; }

    /// <summary>
    /// Стоимость со скидкой, округлённая до двух знаков.
    /// </summary>
    public decimal Price { get; set; }
}