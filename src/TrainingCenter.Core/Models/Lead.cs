namespace TrainingCenter.Core.Models;

using TrainingCenter.Core.Enums;

/// <summary>
/// Сохранённая заявка и состояние её отправки.
/// </summary>
public class Lead
{
    /// <summary>
    /// Идентификатор заявки.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Имя клиента или наименование организации.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Телефон для обратного звонка.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Адрес электронной почты клиента.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Тип клиента, определяющий резервного получателя.
    /// </summary>
    public ClientType ClientType { get; set; }

    /// <summary>
    /// Момент создания заявки в UTC.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Состояние отправки уведомления.
    /// </summary>
    public DeliveryStatus DeliveryStatus { get; set; }

    /// <summary>
    /// Адрес получателя успешно отправленного уведомления; до отправки отсутствует.
    /// </summary>
    public string? Recipient { get; set; }

    /// <summary>
    /// Message-Id успешно отправленного письма; до отправки отсутствует.
    /// </summary>
    public string? MessageId { get; set; }
}