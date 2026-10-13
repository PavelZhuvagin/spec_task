namespace TrainingCenter.Tests.ControllerTest;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TrainingCenter.BusinessLogic.Models.Leads;
using TrainingCenter.BusinessLogic.Models.Mail;
using TrainingCenter.BusinessLogic.Services;
using TrainingCenter.BusinessLogic.Settings;
using TrainingCenter.Core.Enums;
using TrainingCenter.Core.Models;
using TrainingCenter.Core.Storage;
using TrainingCenter.DataAccess.Mail;
using TrainingCenter.Tests.Infrastructure;
using TrainingCenter.WebApi.Controllers;
using Xunit;

/// <summary>
/// Модульный тест контроллера <see cref="LeadsController"/>.
/// </summary>
public class LeadsControllerUnitTest
{
    /// <summary>
    /// Тест метода <see cref="LeadsController.CreateAsync"/>.
    /// Проверяет успешный ответ контроллера и отправку сохранённой заявки нужному получателю.
    /// </summary>
    /// <param name="clientType">Тип клиента тестовой заявки.</param>
    /// <param name="simulateFailure">Признак имитации сбоя основного канала.</param>
    /// <param name="recipient">Ожидаемый адрес получателя уведомления.</param>
    /// <param name="channel">Ожидаемый канал отправки.</param>
    /// <returns>Задача выполнения теста.</returns>
    [Theory]
    [InlineData(ClientType.Individual, true, "mail2@yandex.ru", MailChannel.Fallback)]
    [InlineData(ClientType.LegalEntity, true, "mail1@yandex.ru", MailChannel.Fallback)]
    [InlineData(ClientType.Individual, false, "training-center@yandex.ru", MailChannel.Primary)]
    public async Task CreateLeadTest(ClientType clientType, bool simulateFailure, string recipient, MailChannel channel)
    {
        var repository = new MemoryLeadRepository();
        var unitOfWork = new MemoryUnitOfWork(repository);
        var transport = new DemoMailTransport();
        var settings = Options.Create(new MailSettings { SimulatePrimaryFailure = simulateFailure });
        var service = new LeadService(unitOfWork, transport, settings, new FixedTimeProvider());
        var controller = new LeadsController(service, NullLogger<LeadsController>.Instance);

        var result = await controller.CreateAsync(new CreateLeadRequest
        {
            Name = "Иван",
            Phone = "+79991234567",
            Email = "ivan@example.com",
            ClientType = clientType,
        }, CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CreateLeadResponseDto>(response.Value);
        var lead = Assert.Single(repository.Leads);
        var sent = Assert.Single(transport.Messages);
        Assert.Equal(lead.Id, dto.Id);
        Assert.Equal(2, unitOfWork.SaveCalls);
        Assert.Equal(channel, sent.Channel);
        Assert.Equal(recipient, sent.Message.Recipient);
        Assert.Equal(recipient, lead.Recipient);
        Assert.Equal(channel == MailChannel.Primary ? DeliveryStatus.PrimarySent : DeliveryStatus.FallbackSent, lead.DeliveryStatus);
    }

    /// <summary>
    /// Тестовая единица работы с репозиторием заявок и счётчиком сохранений.
    /// </summary>
    private class MemoryUnitOfWork : IUnitOfWork
    {
        private readonly MemoryLeadRepository repository;

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="repository">Тестовый репозиторий заявок.</param>
        public MemoryUnitOfWork(MemoryLeadRepository repository)
        {
            this.repository = repository;
        }

        /// <summary>
        /// Количество явных сохранений, выполненных сервисом.
        /// </summary>
        public int SaveCalls { get; private set; }

        /// <inheritdoc />
        public IRepository<TEntity> GetRepository<TEntity>()
            where TEntity : class
        {
            return (IRepository<TEntity>)(object)this.repository;
        }

        /// <inheritdoc />
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.SaveCalls++;
            return Task.FromResult(1);
        }
    }

    /// <summary>
    /// Тестовый репозиторий, хранящий отслеживаемые заявки в коллекции.
    /// </summary>
    private class MemoryLeadRepository : IRepository<Lead>
    {
        /// <summary>
        /// Заявки, добавленные при выполнении теста.
        /// </summary>
        public List<Lead> Leads { get; } = new();

        /// <inheritdoc />
        public Task AddAsync(Lead entity, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.Leads.Add(entity);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Update(Lead entity)
        {
            var index = this.Leads.FindIndex(lead => lead.Id == entity.Id);
            if (index >= 0)
            {
                this.Leads[index] = entity;
            }
        }
    }
}