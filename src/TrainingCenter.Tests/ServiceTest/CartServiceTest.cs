namespace TrainingCenter.Tests.ServiceTest;

using TrainingCenter.BusinessLogic.Services;
using TrainingCenter.Core.Models;
using TrainingCenter.Tests.Infrastructure;
using Xunit;

/// <summary>
/// Тест сервиса <see cref="CartService"/>.
/// </summary>
public class CartServiceTest
{
    /// <summary>
    /// Тест метода <see cref="CartService.Calculate"/>.
    /// Проверяет максимальную скидку, границу пяти суток, округление и пустую корзину.
    /// </summary>
    /// <param name="isGraduate">Признак выпускника в тестовом профиле.</param>
    /// <param name="days">Количество суток до старта первого курса.</param>
    /// <param name="discount">Ожидаемая скидка первого курса в процентах.</param>
    /// <param name="expected">Ожидаемая стоимость первого курса со скидкой.</param>
    [Theory]
    [InlineData(false, 4.99, 20, 800)]
    [InlineData(false, 5, 0, 1000)]
    [InlineData(false, 6, 0, 1000)]
    [InlineData(false, -1, 0, 1000)]
    [InlineData(false, 0, 20, 800)]
    [InlineData(true, 1, 30, 700)]
    [InlineData(true, 5, 30, 700)]
    public void CalculateTest(bool isGraduate, double days, int discount, decimal expected)
    {
        var clock = new FixedTimeProvider();
        var service = new CartService(clock);
        var first = new CourseDto
        {
            Id = Guid.NewGuid(),
            Name = "C#",
            Price = 1000m,
            StartsAt = clock.GetUtcNow().AddDays(days),
        };
        var second = new CourseDto
        {
            Id = Guid.NewGuid(),
            Name = "SQL",
            Price = 100.01m,
            StartsAt = clock.GetUtcNow().AddDays(10),
        };

        var result = service.Calculate(new[] { first, second }, new UserProfileDto { IsGraduate = isGraduate });

        Assert.Equal(discount, result.Courses[0].DiscountPercent);
        Assert.Equal(expected, result.Courses[0].Price);
        Assert.Equal(isGraduate ? 30 : 0, result.Courses[1].DiscountPercent);
        Assert.Equal(expected + (isGraduate ? 70.01m : 100.01m), result.Total);
        Assert.Equal(0m, service.Calculate(Array.Empty<CourseDto>(), new UserProfileDto { IsGraduate = isGraduate }).Total);
    }
}