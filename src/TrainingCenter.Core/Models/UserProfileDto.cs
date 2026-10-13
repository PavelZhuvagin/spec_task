namespace TrainingCenter.Core.Models;

/// <summary>
/// Статус пользователя для расчёта скидок.
/// </summary>
public class UserProfileDto
{
    /// <summary>
    /// Признак выпускника, которому доступна скидка 30%.
    /// </summary>
    public bool IsGraduate { get; set; }
}