namespace TrainingCenter.Tests.ControllerTest;

using System.Net;
using System.Net.Http.Json;
using System.Globalization;
using System.Resources;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TrainingCenter.BusinessLogic.Models.Mail;
using TrainingCenter.BusinessLogic.Services.Interfaces;
using TrainingCenter.Core.Enums;
using TrainingCenter.Core.Exceptions;
using TrainingCenter.Core.Resources;
using TrainingCenter.DataAccess;
using TrainingCenter.DataAccess.Mail;
using TrainingCenter.WebApi.Controllers;
using Xunit;

/// <summary>
/// Интеграционный тест контроллера <see cref="LeadsController"/>.
/// </summary>
public class LeadsControllerTest
{
    /// <summary>
    /// Тест метода <see cref="LeadsController.CreateAsync"/>.
    /// Проверяет 200 OK и сохранение результата резервной доставки при сбое основного канала.
    /// </summary>
    /// <param name="clientType">Строковый тип клиента в JSON-запросе.</param>
    /// <param name="recipient">Ожидаемый резервный адрес получателя.</param>
    /// <returns>Задача выполнения теста.</returns>
    [Theory]
    [InlineData("Individual", "mail2@yandex.ru")]
    [InlineData("LegalEntity", "mail1@yandex.ru")]
    public async Task CreateLeadWithPrimaryFailureTest(string clientType, string recipient)
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/leads", new
        {
            name = "Иван",
            phone = "+79991234567",
            email = "ivan@example.com", clientType,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Trace-Id"));
        using var scope = factory.Services.CreateScope();
        var lead = Assert.Single(await scope.ServiceProvider.GetRequiredService<TrainingCenterDbContext>().Leads.ToListAsync());
        Assert.Equal(DeliveryStatus.FallbackSent, lead.DeliveryStatus);
        Assert.Equal(recipient, lead.Recipient);
        var message = Assert.Single(factory.Services.GetRequiredService<DemoMailTransport>().Messages);
        Assert.Equal(recipient, message.Message.Recipient);
    }

    /// <summary>
    /// Тест метода <see cref="LeadsController.CreateAsync"/>.
    /// Проверяет 400 Bad Request без сохранения заявки и отправки письма.
    /// </summary>
    /// <param name="phone">Телефон тестовой заявки.</param>
    /// <param name="email">Email тестовой заявки.</param>
    /// <param name="clientType">Строковый тип клиента в JSON-запросе.</param>
    /// <returns>Задача выполнения теста.</returns>
    [Theory]
    [InlineData("", "ivan@example.com", "Individual")]
    [InlineData("+79991234567", "invalid", "Individual")]
    [InlineData("+79991234567", "ivan@example.com", "Unknown")]
    public async Task CreateLeadWithInvalidDataTest(string phone, string email, string clientType)
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/leads", new { name = "Иван", phone, email, clientType });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        Assert.Empty(await scope.ServiceProvider.GetRequiredService<TrainingCenterDbContext>().Leads.ToListAsync());
        Assert.Empty(factory.Services.GetRequiredService<DemoMailTransport>().Messages);
    }

    /// <summary>
    /// Тест метода <see cref="LeadsController.CreateAsync"/>.
    /// Проверяет безопасный ответ 503 и сохранение заявки со статусом Failed при отказе обоих каналов.
    /// </summary>
    /// <returns>Задача выполнения теста.</returns>
    [Fact]
    public async Task CreateLeadWithUnavailableMailTest()
    {
        using var factory = new ApiFactory(new FailingTransport());
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/leads", new
        {
            name = "Иван",
            phone = "+79991234567",
            email = "ivan@example.com",
            clientType = "Individual",
        });

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("traceId", body);
        Assert.DoesNotContain(Messages.SmtpUnavailable, body);
        using var scope = factory.Services.CreateScope();
        var lead = Assert.Single(await scope.ServiceProvider.GetRequiredService<TrainingCenterDbContext>().Leads.ToListAsync());
        Assert.Equal(DeliveryStatus.Failed, lead.DeliveryStatus);
    }

    /// <summary>
    /// Тест метода <see cref="LeadsController.CreateAsync"/>.
    /// Проверяет ответ 500 без раскрытия деталей неожиданного исключения.
    /// </summary>
    /// <returns>Задача выполнения теста.</returns>
    [Fact]
    public async Task CreateLeadWithUnexpectedFailureTest()
    {
        using var factory = new ApiFactory(new UnexpectedFailureTransport());
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/leads", new
        {
            name = "Иван",
            phone = "+79991234567",
            email = "ivan@example.com",
            clientType = "Individual",
        });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain(Messages.InvalidMailMode, await response.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// Тест метода <see cref="LeadsController.CreateAsync"/>.
    /// Проверяет выбор культуры и перевод подтверждения, валидации и ошибки доставки.
    /// </summary>
    /// <param name="acceptLanguage">Значение заголовка Accept-Language.</param>
    /// <param name="expectedCulture">Ожидаемый язык ресурсов и ответа.</param>
    /// <param name="expectedStatus">Ожидаемый статус моделируемого сценария.</param>
    /// <returns>Задача выполнения теста локализации HTTP-ответа.</returns>
    [Theory]
    [InlineData("ru", "ru", HttpStatusCode.OK)]
    [InlineData("en", "en", HttpStatusCode.OK)]
    [InlineData("ru-RU", "ru", HttpStatusCode.OK)]
    [InlineData("en-US", "en", HttpStatusCode.OK)]
    [InlineData("de", "ru", HttpStatusCode.OK)]
    [InlineData("ru", "ru", HttpStatusCode.BadRequest)]
    [InlineData("en", "en", HttpStatusCode.BadRequest)]
    [InlineData("ru", "ru", HttpStatusCode.ServiceUnavailable)]
    [InlineData("en", "en", HttpStatusCode.ServiceUnavailable)]
    public async Task CreateLeadLocalizationTest(string acceptLanguage, string expectedCulture, HttpStatusCode expectedStatus)
    {
        using var factory = new ApiFactory(expectedStatus == HttpStatusCode.ServiceUnavailable ? new FailingTransport() : null);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(acceptLanguage);
        var response = await client.PostAsJsonAsync("/api/leads", new
        {
            name = "Иван",
            phone = expectedStatus == HttpStatusCode.BadRequest ? "" : "+79991234567",
            email = expectedStatus == HttpStatusCode.BadRequest ? "invalid" : "ivan@example.com",
            clientType = "Individual",
        });

        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Contains(expectedCulture, response.Content.Headers.ContentLanguage);
        var resourceManager = new ResourceManager("TrainingCenter.Core.Resources.Messages", typeof(Messages).Assembly);
        var culture = CultureInfo.GetCultureInfo(expectedCulture);
        var key = expectedStatus switch
        {
            HttpStatusCode.OK => nameof(Messages.LeadAccepted),
            HttpStatusCode.BadRequest => nameof(Messages.ModelValidationTitle),
            _ => nameof(Messages.DeliveryUnavailableTitle),
        };
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(resourceManager.GetString(key, culture), body.RootElement.GetProperty(expectedStatus == HttpStatusCode.OK ? "message" : "title").GetString());
        if (expectedStatus == HttpStatusCode.BadRequest)
        {
            var emailError = string.Format(culture, resourceManager.GetString(nameof(Messages.InvalidEmail), culture)!, resourceManager.GetString(nameof(Messages.EmailField), culture));
            var phoneError = string.Format(culture, resourceManager.GetString(nameof(Messages.RequiredField), culture)!, resourceManager.GetString(nameof(Messages.PhoneField), culture));
            var errors = body.RootElement.GetProperty("errors").EnumerateObject().SelectMany(error => error.Value.EnumerateArray()).Select(error => error.GetString()).ToArray();
            Assert.Contains(emailError, errors);
            Assert.Contains(phoneError, errors);
        }
    }

    /// <summary>
    /// Тест метода <see cref="LeadsController.CreateAsync"/>.
    /// Проверяет локализацию ошибок разбора JSON и отсутствие исходных сообщений сериализатора.
    /// </summary>
    /// <param name="cultureName">Язык ответа API.</param>
    /// <param name="invalidJson">Некорректное JSON-тело запроса.</param>
    /// <returns>Задача выполнения теста локализации ошибок привязки модели.</returns>
    [Theory]
    [InlineData("ru", "{")]
    [InlineData("en", "{")]
    [InlineData("ru", "")]
    [InlineData("en", "")]
    [InlineData("ru", "null")]
    [InlineData("en", "null")]
    [InlineData("ru", "{\"name\":\"Ivan\",\"phone\":\"+79991234567\",\"email\":\"ivan@example.com\",\"clientType\":\"Unknown\"}")]
    [InlineData("en", "{\"name\":\"Ivan\",\"phone\":\"+79991234567\",\"email\":\"ivan@example.com\",\"clientType\":\"Unknown\"}")]
    public async Task CreateLeadWithInvalidJsonTest(string cultureName, string invalidJson)
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(cultureName);
        using var content = new StringContent(invalidJson, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/leads", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var manager = new ResourceManager("TrainingCenter.Core.Resources.Messages", typeof(Messages).Assembly);
        var expected = manager.GetString(nameof(Messages.ModelBindingError), CultureInfo.GetCultureInfo(cultureName));
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = body.RootElement.GetProperty("errors").EnumerateObject().SelectMany(error => error.Value.EnumerateArray()).ToArray();
        Assert.NotEmpty(errors);
        Assert.All(errors, error => Assert.Equal(expected, error.GetString()));
    }

    /// <summary>
    /// Тест стандартных HTTP-ошибок, локализации и идентификатора трассировки.
    /// </summary>
    /// <param name="cultureName">Язык ответа.</param>
    /// <param name="statusCode">Ожидаемый статус стандартной HTTP-ошибки.</param>
    /// <returns>Задача выполнения теста.</returns>
    [Theory]
    [InlineData("ru", HttpStatusCode.NotFound)]
    [InlineData("en", HttpStatusCode.NotFound)]
    [InlineData("ru", HttpStatusCode.MethodNotAllowed)]
    [InlineData("en", HttpStatusCode.MethodNotAllowed)]
    [InlineData("ru", HttpStatusCode.UnsupportedMediaType)]
    [InlineData("en", HttpStatusCode.UnsupportedMediaType)]
    public async Task StandardHttpErrorLocalizationTest(string cultureName, HttpStatusCode statusCode)
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        const string traceId = "00112233445566778899aabbccddeeff";
        client.DefaultRequestHeaders.Add("traceparent", $"00-{traceId}-0123456789abcdef-01");
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(cultureName);
        using var request = new HttpRequestMessage(
            statusCode == HttpStatusCode.UnsupportedMediaType ? HttpMethod.Post : HttpMethod.Get,
            statusCode == HttpStatusCode.NotFound ? "/missing" : "/api/leads");
        if (statusCode == HttpStatusCode.UnsupportedMediaType)
        {
            request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "text/plain");
        }

        var response = await client.SendAsync(request);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains(cultureName, response.Content.Headers.ContentLanguage);
        Assert.Equal(traceId, Assert.Single(response.Headers.GetValues("X-Trace-Id")));
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(traceId, body.RootElement.GetProperty("traceId").GetString());
        var key = statusCode switch
        {
            HttpStatusCode.NotFound => nameof(Messages.NotFound),
            HttpStatusCode.MethodNotAllowed => nameof(Messages.MethodNotAllowed),
            _ => nameof(Messages.UnsupportedMediaType),
        };
        var resources = new ResourceManager("TrainingCenter.Core.Resources.Messages", typeof(Messages).Assembly);
        Assert.Equal(resources.GetString(key, CultureInfo.GetCultureInfo(cultureName)), body.RootElement.GetProperty("title").GetString());
    }

    /// <summary>
    /// Тест единого TraceId в ответах валидации и обработчика исключений.
    /// </summary>
    /// <param name="statusCode">Ожидаемый статус ответа.</param>
    /// <returns>Задача выполнения теста.</returns>
    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task ErrorTraceIdTest(HttpStatusCode statusCode)
    {
        IMailTransport? transport = statusCode switch
        {
            HttpStatusCode.ServiceUnavailable => new FailingTransport(),
            HttpStatusCode.InternalServerError => new UnexpectedFailureTransport(),
            _ => null,
        };
        using var factory = new ApiFactory(transport);
        using var client = factory.CreateClient();
        const string traceId = "ffeeddccbbaa99887766554433221100";
        client.DefaultRequestHeaders.Add("traceparent", $"00-{traceId}-0123456789abcdef-01");
        var response = await client.PostAsJsonAsync("/api/leads", new
        {
            name = "Trace test",
            phone = statusCode == HttpStatusCode.BadRequest ? string.Empty : "+79991234567",
            email = "audit@example.com",
            clientType = "Individual",
        });

        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(traceId, Assert.Single(response.Headers.GetValues("X-Trace-Id")));
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(traceId, body.RootElement.GetProperty("traceId").GetString());
    }

    /// <summary>
    /// Создаёт изолированный тестовый хост с отдельной InMemory-базой и заменяемым транспортом.
    /// </summary>
    private class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly IMailTransport? transport;
        private readonly string databaseName = Guid.NewGuid().ToString();

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="transport">Тестовый транспорт; при отсутствии используется демотранспорт.</param>
        public ApiFactory(IMailTransport? transport = null)
        {
            this.transport = transport;
        }

        /// <summary>
        /// Задаёт деморежим, отдельное хранилище и при необходимости тестовый почтовый транспорт.
        /// </summary>
        /// <param name="builder">Конструктор тестового Web API-хоста.</param>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Mail:Mode"] = "Demo",
                ["Mail:SimulatePrimaryFailure"] = "true",
                ["Mail:LegalEntityRecipient"] = "mail1@yandex.ru",
                ["Mail:IndividualRecipient"] = "mail2@yandex.ru",
            }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<TrainingCenterDbContext>>();
                services.AddDbContext<TrainingCenterDbContext>(options => options.UseInMemoryDatabase(this.databaseName));
                if (this.transport != null)
                {
                    services.RemoveAll<IMailTransport>();
                    services.AddSingleton(this.transport);
                }
            });
        }
    }

    /// <summary>
    /// Тестовый транспорт, имитирующий ожидаемую ошибку доставки.
    /// </summary>
    private class FailingTransport : IMailTransport
    {
        /// <summary>
        /// Имитирует недоступность почтового транспорта.
        /// </summary>
        /// <param name="channel">Канал тестовой отправки.</param>
        /// <param name="message">Письмо, которое не будет отправлено.</param>
        /// <param name="cancellationToken">Токен отмены тестовой отправки.</param>
        /// <returns>Задача отправки; метод всегда выбрасывает исключение.</returns>
        /// <exception cref="MailDeliveryException">Выбрасывается при каждом вызове для проверки ответа 503.</exception>
        public Task SendAsync(MailChannel channel, OutgoingMailDto message, CancellationToken cancellationToken) =>
            throw new MailDeliveryException(Messages.SmtpUnavailable);
    }

    /// <summary>
    /// Тестовый транспорт, имитирующий неожиданную ошибку приложения.
    /// </summary>
    private class UnexpectedFailureTransport : IMailTransport
    {
        /// <summary>
        /// Имитирует ошибку, не относящуюся к доступности почтового сервиса.
        /// </summary>
        /// <param name="channel">Канал тестовой отправки.</param>
        /// <param name="message">Письмо, которое не будет отправлено.</param>
        /// <param name="cancellationToken">Токен отмены тестовой отправки.</param>
        /// <returns>Задача отправки; метод всегда выбрасывает исключение.</returns>
        /// <exception cref="InvalidOperationException">Выбрасывается при каждом вызове для проверки ответа 500.</exception>
        public Task SendAsync(MailChannel channel, OutgoingMailDto message, CancellationToken cancellationToken) =>
            throw new InvalidOperationException(Messages.InvalidMailMode);
    }
}