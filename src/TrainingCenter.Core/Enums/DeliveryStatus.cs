namespace TrainingCenter.Core.Enums;

/// <summary>
/// Состояние доставки уведомления о заявке.
/// </summary>
public enum DeliveryStatus
{
    /// <summary>
    /// Заявка сохранена и ожидает отправки.
    /// </summary>
    Pending,

    /// <summary>
    /// Уведомление принято основным почтовым каналом.
    /// </summary>
    PrimarySent,

    /// <summary>
    /// Уведомление принято резервным почтовым каналом.
    /// </summary>
    FallbackSent,

    /// <summary>
    /// Резервный канал также недоступен; заявка остаётся в хранилище.
    /// </summary>
    Failed,
}