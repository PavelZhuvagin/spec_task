namespace TrainingCenter.Core.Exceptions;

using TrainingCenter.Core.Resources;

/// <summary>
/// Заявка сохранена, но уведомление не удалось отправить ни одним каналом.
/// </summary>
public class DeliveryUnavailableException : Exception
{
    /// <summary>
    /// ctor.
    /// </summary>
    /// <param name="innerException">Причина отказа резервного почтового канала.</param>
    public DeliveryUnavailableException(Exception innerException)
        : base(Messages.DeliveryUnavailable, innerException)
    {
    }
}