namespace TrainingCenter.BusinessLogic.Models.Mail;

/// <summary>
/// Содержимое исходящего текстового письма.
/// </summary>
public class OutgoingMailDto
{
    /// <summary>
    /// Адрес получателя.
    /// </summary>
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// Тема письма.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Тело письма в формате plain text.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор письма для связи с заявкой и поиска через IMAP.
    /// </summary>
    public string MessageId { get; set; } = string.Empty;
}