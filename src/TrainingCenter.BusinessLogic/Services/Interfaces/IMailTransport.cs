namespace TrainingCenter.BusinessLogic.Services.Interfaces;

using TrainingCenter.BusinessLogic.Models.Mail;

/// <summary>
/// Транспорт отправки писем через выбранный почтовый канал.
/// </summary>
public interface IMailTransport
{
    /// <summary>
    /// Передаёт письмо выбранному транспорту.
    /// </summary>
    /// <param name="channel">Основной либо резервный канал.</param>
    /// <param name="message">Получатель и содержимое исходящего письма.</param>
    /// <param name="cancellationToken">Токен отмены отправки.</param>
    /// <returns>Задача передачи письма; подтверждение транспорта не гарантирует попадание во входящие.</returns>
    /// <exception cref="TrainingCenter.Core.Exceptions.MailDeliveryException">Почтовый транспорт не принял письмо.</exception>
    Task SendAsync(MailChannel channel, OutgoingMailDto message, CancellationToken cancellationToken);
}