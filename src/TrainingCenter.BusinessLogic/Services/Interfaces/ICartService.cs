namespace TrainingCenter.BusinessLogic.Services.Interfaces;

using TrainingCenter.BusinessLogic.Models.Cart;
using TrainingCenter.Core.Models;

/// <summary>
/// Расчёт стоимости корзины по курсам и статусу пользователя.
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Выбирает максимальную скидку для каждого курса и суммирует округлённые цены.
    /// </summary>
    /// <param name="courses">Курсы корзины с неотрицательными ценами.</param>
    /// <param name="user">Профиль пользователя с признаком выпускника.</param>
    /// <returns>Стоимость корзины и разбивка по курсам; для пустой корзины итог равен нулю.</returns>
    /// <exception cref="ArgumentNullException">Коллекция, профиль или отдельный курс отсутствует.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Цена курса отрицательная.</exception>
    /// <remarks>
    /// Скидка 20% доступна, если до старта осталось от нуля до строго менее пяти суток; выпускнику доступно 30%. Цены округляются до двух знаков с AwayFromZero.
    /// </remarks>
    CartTotalDto Calculate(IReadOnlyCollection<CourseDto> courses, UserProfileDto user);
}