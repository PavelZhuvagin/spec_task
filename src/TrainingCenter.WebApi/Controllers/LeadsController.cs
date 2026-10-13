namespace TrainingCenter.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using TrainingCenter.BusinessLogic.Models.Leads;
using TrainingCenter.BusinessLogic.Services.Interfaces;
using TrainingCenter.Core.Exceptions;
using TrainingCenter.Core.Resources;

/// <summary>
/// Обработка формы обратного звонка.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LeadsController : ControllerBase
{
    private readonly ILeadService leadService;
    private readonly ILogger<LeadsController> logger;

    /// <summary>
    /// ctor.
    /// </summary>
    /// <param name="leadService">Сервис сохранения и отправки заявок.</param>
    /// <param name="logger">Логгер переключения на резервный почтовый канал.</param>
    public LeadsController(ILeadService leadService, ILogger<LeadsController> logger)
    {
        this.leadService = leadService;
        this.logger = logger;
    }

    /// <summary>
    /// Сохраняет заявку и отправляет уведомление через основной либо резервный канал.
    /// </summary>
    /// <param name="request">Данные клиента.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Номер принятой заявки.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateLeadResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateLeadResponseDto>> CreateAsync([FromBody] CreateLeadRequest request, CancellationToken cancellationToken)
    {
        var lead = await this.leadService.CreateAsync(request, cancellationToken);
        try
        {
            await this.leadService.SendPrimaryAsync(lead, cancellationToken);
        }
        catch (MailDeliveryException exception)
        {
            this.logger.LogWarning(exception, Messages.PrimaryFallbackLog, lead.Id);
            await this.leadService.SendFallbackAsync(lead, cancellationToken);
        }

        return this.Ok(new CreateLeadResponseDto { Id = lead.Id });
    }
}