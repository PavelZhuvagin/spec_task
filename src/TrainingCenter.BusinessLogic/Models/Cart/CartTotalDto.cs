namespace TrainingCenter.BusinessLogic.Models.Cart;

/// <summary>
/// Стоимость корзины с разбивкой по курсам.
/// </summary>
public class CartTotalDto
{
    /// <summary>
    /// Результаты расчёта отдельных курсов.
    /// </summary>
    public IReadOnlyList<CourseTotalDto> Courses { get; set; } = Array.Empty<CourseTotalDto>();

    /// <summary>
    /// Сумма округлённых стоимостей курсов со скидками.
    /// </summary>
    public decimal Total { get; set; }
}