namespace TrainingCenter.Core.Exceptions;

/// <summary>
/// Ошибка почтового транспорта, при которой допустим резервный канал.
/// </summary>
public class MailDeliveryException : Exception
{
    /// <summary>
    /// ctor.
    /// </summary>
    /// <param name="message">Описание ошибки отправки.</param>
    /// <param name="innerException">Исходная ошибка транспорта, если она известна.</param>
    public MailDeliveryException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}