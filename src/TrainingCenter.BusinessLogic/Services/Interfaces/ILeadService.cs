namespace TrainingCenter.BusinessLogic.Services.Interfaces;

using TrainingCenter.BusinessLogic.Models.Leads;
using TrainingCenter.Core.Models;

/// <summary>
/// Создание заявок и отправка уведомлений по основному и резервному каналам.
/// </summary>
public interface ILeadService
{
    /// <summary>
    /// Проверяет входные данные и сохраняет заявку в состоянии ожидания отправки.
    /// </summary>
    /// <param name="request">Данные формы обратного звонка.</param>
    /// <param name="cancellationToken">Токен отмены сохранения.</param>
    /// <returns>Сохранённая заявка с идентификатором и временем создания.</returns>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">Данные формы не прошли валидацию.</exception>
    Task<Lead> CreateAsync(CreateLeadRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Отправляет уведомление основному получателю и сохраняет состояние доставки.
    /// </summary>
    /// <param name="lead">Сохранённая заявка.</param>
    /// <param name="cancellationToken">Токен отмены отправки и сохранения.</param>
    /// <returns>Задача отправки уведомления и обновления заявки.</returns>
    /// <exception cref="TrainingCenter.Core.Exceptions.MailDeliveryException">Основной канал недоступен либо включена имитация сбоя.</exception>
    Task SendPrimaryAsync(Lead lead, CancellationToken cancellationToken);

    /// <summary>
    /// Отправляет уведомление резервному получателю согласно типу клиента.
    /// </summary>
    /// <param name="lead">Сохранённая заявка.</param>
    /// <param name="cancellationToken">Токен отмены отправки и сохранения.</param>
    /// <returns>Задача отправки уведомления и обновления заявки.</returns>
    /// <exception cref="TrainingCenter.Core.Exceptions.DeliveryUnavailableException">Резервный транспорт недоступен; заявка сохранена со статусом Failed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Тип клиента не поддерживается.</exception>
    Task SendFallbackAsync(Lead lead, CancellationToken cancellationToken);
}