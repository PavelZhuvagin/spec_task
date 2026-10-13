namespace TrainingCenter.BusinessLogic.Services.Interfaces;

using TrainingCenter.BusinessLogic.Models.Mail;

/// <summary>
/// Проверка наличия письма во входящих выбранного почтового аккаунта.
/// </summary>
public interface IMailInboxService
{
    /// <summary>
    /// Ищет письмо по Message-Id без изменения флагов прочтения.
    /// </summary>
    /// <param name="channel">Канал, определяющий почтовый аккаунт.</param>
    /// <param name="messageId">Идентификатор искомого письма.</param>
    /// <param name="cancellationToken">Токен отмены поиска.</param>
    /// <returns>Задача с результатом true, если письмо найдено, иначе false.</returns>
    Task<bool> ContainsAsync(MailChannel channel, string messageId, CancellationToken cancellationToken);
}