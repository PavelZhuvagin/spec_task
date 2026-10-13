namespace TrainingCenter.BusinessLogic.Services;

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using TrainingCenter.BusinessLogic.Models.Leads;
using TrainingCenter.BusinessLogic.Models.Mail;
using TrainingCenter.BusinessLogic.Services.Interfaces;
using TrainingCenter.BusinessLogic.Settings;
using TrainingCenter.Core.Enums;
using TrainingCenter.Core.Exceptions;
using TrainingCenter.Core.Models;
using TrainingCenter.Core.Resources;
using TrainingCenter.Core.Storage;

/// <summary>
/// Сохраняет заявку до отправки и выбирает адрес резервного получателя.
/// </summary>
public class LeadService : ILeadService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IRepository<Lead> repository;
    private readonly IMailTransport mailTransport;
    private readonly MailSettings settings;
    private readonly TimeProvider timeProvider;

    /// <summary>
    /// ctor.
    /// </summary>
    /// <param name="unitOfWork">Единица работы с репозиторием заявок и общим сохранением изменений.</param>
    /// <param name="mailTransport">Транспорт основного и резервного каналов.</param>
    /// <param name="settings">Настройки отправки и адресов получателей.</param>
    /// <param name="timeProvider">Источник времени создания заявки.</param>
    public LeadService(IUnitOfWork unitOfWork, IMailTransport mailTransport, IOptions<MailSettings> settings, TimeProvider timeProvider)
    {
        this.unitOfWork = unitOfWork;
        this.repository = unitOfWork.GetRepository<Lead>();
        this.mailTransport = mailTransport;
        this.settings = settings.Value;
        this.timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<Lead> CreateAsync(CreateLeadRequest request, CancellationToken cancellationToken)
    {
        Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email.Trim(),
            ClientType = request.ClientType!.Value,
            CreatedAt = this.timeProvider.GetUtcNow(),
            DeliveryStatus = DeliveryStatus.Pending,
        };

        await this.repository.AddAsync(lead, cancellationToken);
        await this.unitOfWork.SaveChangesAsync(cancellationToken);
        return lead;
    }

    /// <inheritdoc />
    public async Task SendPrimaryAsync(Lead lead, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (this.settings.SimulatePrimaryFailure)
        {
            throw new MailDeliveryException(Messages.PrimaryFailureSimulation);
        }

        await this.SendAsync(lead, MailChannel.Primary, this.settings.PrimaryRecipient, DeliveryStatus.PrimarySent, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SendFallbackAsync(Lead lead, CancellationToken cancellationToken)
    {
        var recipient = lead.ClientType switch
        {
            ClientType.LegalEntity => this.settings.LegalEntityRecipient,
            ClientType.Individual => this.settings.IndividualRecipient,
            _ => throw new ArgumentOutOfRangeException(nameof(lead), Messages.UnsupportedClient),
        };

        try
        {
            await this.SendAsync(lead, MailChannel.Fallback, recipient, DeliveryStatus.FallbackSent, cancellationToken);
        }
        catch (MailDeliveryException exception)
        {
            lead.DeliveryStatus = DeliveryStatus.Failed;
            this.repository.Update(lead);
            await this.unitOfWork.SaveChangesAsync(cancellationToken);
            throw new DeliveryUnavailableException(exception);
        }
    }

    private async Task SendAsync(Lead lead, MailChannel channel, string recipient, DeliveryStatus status, CancellationToken cancellationToken)
    {
        var messageId = $"lead-{lead.Id:N}@training-center.local";
        var clientType = lead.ClientType == ClientType.LegalEntity ? Messages.LegalEntityClient : Messages.IndividualClient;
        var message = new OutgoingMailDto
        {
            Recipient = recipient,
            Subject = Messages.Format(Messages.LeadSubject, lead.Id),
            Body = Messages.Format(Messages.LeadBody, lead.Name, lead.Phone, lead.Email, clientType, lead.Id),
            MessageId = messageId,
        };

        await this.mailTransport.SendAsync(channel, message, cancellationToken);
        lead.Recipient = recipient;
        lead.MessageId = messageId;
        lead.DeliveryStatus = status;
        this.repository.Update(lead);
        await this.unitOfWork.SaveChangesAsync(cancellationToken);
    }
}