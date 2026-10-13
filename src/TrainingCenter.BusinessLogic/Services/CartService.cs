namespace TrainingCenter.BusinessLogic.Services;

using TrainingCenter.BusinessLogic.Models.Cart;
using TrainingCenter.BusinessLogic.Services.Interfaces;
using TrainingCenter.Core.Models;
using TrainingCenter.Core.Resources;

/// <summary>
/// Выбирает максимальную скидку отдельно для каждого курса.
/// </summary>
public class CartService : ICartService
{
    private readonly TimeProvider timeProvider;

    /// <summary>
    /// ctor.
    /// </summary>
    /// <param name="timeProvider">Источник текущего времени для определения горящих групп.</param>
    public CartService(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public CartTotalDto Calculate(IReadOnlyCollection<CourseDto> courses, UserProfileDto user)
    {
        if (courses == null)
        {
            throw new ArgumentNullException(nameof(courses), Messages.CoursesRequired);
        }

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), Messages.UserRequired);
        }
        var now = this.timeProvider.GetUtcNow();
        var result = new List<CourseTotalDto>(courses.Count);

        foreach (var course in courses)
        {
            if (course == null)
            {
                throw new ArgumentNullException(nameof(courses), Messages.CourseRequired);
            }

            if (course.Price < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(courses), Messages.NegativePrice);
            }
            var remaining = course.StartsAt - now;
            var hotDiscount = remaining >= TimeSpan.Zero && remaining < TimeSpan.FromDays(5) ? 20 : 0;
            var discount = Math.Max(hotDiscount, user.IsGraduate ? 30 : 0);
            var price = decimal.Round(course.Price * (100 - discount) / 100, 2, MidpointRounding.AwayFromZero);
            result.Add(new CourseTotalDto { CourseId = course.Id, OriginalPrice = course.Price, DiscountPercent = discount, Price = price });
        }

        return new CartTotalDto { Courses = result, Total = result.Sum(course => course.Price) };
    }
}