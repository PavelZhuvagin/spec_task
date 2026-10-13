namespace TrainingCenter.BusinessLogic.Models.Mail;

/// <summary>
/// Почтовый канал отправки или проверки входящих писем.
/// </summary>
public enum MailChannel
{
    /// <summary>
    /// Основной канал Яндекса.
    /// </summary>
    Primary,

    /// <summary>
    /// Резервный канал Gmail.
    /// </summary>
    Fallback,
}