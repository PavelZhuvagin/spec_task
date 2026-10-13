namespace TrainingCenter.BusinessLogic.Models.Leads;

/// <summary>
/// Подтверждение успешного приёма заявки без раскрытия выбранного почтового канала.
/// </summary>
public class CreateLeadResponseDto
{
    /// <summary>
    /// Идентификатор сохранённой заявки.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Локализованное сообщение для клиента.
    /// </summary>
    public string Message { get; set; } = TrainingCenter.Core.Resources.Messages.LeadAccepted;
}