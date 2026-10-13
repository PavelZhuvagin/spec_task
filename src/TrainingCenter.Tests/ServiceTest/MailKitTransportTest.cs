namespace TrainingCenter.Tests.ServiceTest;

using System.Net;
using System.Net.Sockets;
using System.Text;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TrainingCenter.BusinessLogic.Models.Mail;
using TrainingCenter.BusinessLogic.Settings;
using TrainingCenter.Core.Exceptions;
using TrainingCenter.Core.Resources;
using TrainingCenter.DataAccess.Mail;
using Xunit;

/// <summary>
/// Тест SMTP-отправки <see cref="MailKitTransport"/> без сетевых подключений.
/// </summary>
public class MailKitTransportTest
{
    /// <summary>
    /// Тест преобразования ошибок подключения, аутентификации и отправки в ошибку доставки.
    /// </summary>
    /// <param name="stage">Этап возникновения ошибки.</param>
    /// <param name="errorType">Тип ошибки SMTP-клиента.</param>
    /// <returns>Задача выполнения теста.</returns>
    [Theory]
    [InlineData("connect", "io")]
    [InlineData("connect", "socket")]
    [InlineData("authenticate", "authentication")]
    [InlineData("send", "protocol")]
    [InlineData("send", "command")]
    public async Task SendWithTransportFailureTest(string stage, string errorType)
    {
        Exception error = errorType switch
        {
            "io" => new IOException(Messages.SmtpUnavailable),
            "socket" => new SocketException((int)SocketError.ConnectionRefused),
            "authentication" => new MailKit.Security.AuthenticationException(Messages.SmtpUnavailable),
            "protocol" => new SmtpProtocolException(Messages.SmtpUnavailable),
            _ => new SmtpCommandException(SmtpErrorCode.MessageNotAccepted, SmtpStatusCode.TransactionFailed, Messages.SmtpUnavailable),
        };
        var client = new TestSmtpClient();
        if (stage == "connect")
        {
            client.ConnectAction = _ => Task.FromException(error);
        }
        else if (stage == "authenticate")
        {
            client.AuthenticationError = error;
        }
        else
        {
            client.SendError = error;
        }

        var transport = new TestMailKitTransport(CreateSettings(), client);
        var result = await Assert.ThrowsAsync<MailDeliveryException>(() => transport.SendAsync(MailChannel.Primary, CreateMessage(), CancellationToken.None));
        Assert.Same(error, result.InnerException);
        Assert.Equal(Messages.SmtpUnavailable, result.Message);
        Assert.Equal(0, client.DisconnectCalls);
    }

    /// <summary>
    /// Тест выбора канала, TLS и содержимого исходящего письма.
    /// </summary>
    /// <param name="channel">Почтовый канал.</param>
    /// <param name="port">SMTP-порт выбранного аккаунта.</param>
    /// <param name="security">Ожидаемый способ подключения TLS.</param>
    /// <returns>Задача выполнения теста.</returns>
    [Theory]
    [InlineData(MailChannel.Primary, 465, SecureSocketOptions.SslOnConnect)]
    [InlineData(MailChannel.Fallback, 587, SecureSocketOptions.StartTls)]
    public async Task SendWithTlsTest(MailChannel channel, int port, SecureSocketOptions security)
    {
        var settings = CreateSettings();
        var account = settings.Value.GetAccount(channel);
        account.SmtpPort = port;
        var client = new TestSmtpClient();
        var message = CreateMessage();
        var transport = new TestMailKitTransport(settings, client);

        await transport.SendAsync(channel, message, CancellationToken.None);

        Assert.Equal(account.SmtpHost, client.Host);
        Assert.Equal(port, client.Port);
        Assert.Equal(security, client.Security);
        Assert.Equal(account.FromAddress, Assert.Single(client.SentMessage!.From.Mailboxes).Address);
        Assert.Equal(message.Recipient, Assert.Single(client.SentMessage.To.Mailboxes).Address);
        Assert.Equal(message.Subject, client.SentMessage.Subject);
        Assert.Equal(message.Body, client.SentMessage.TextBody);
        Assert.Equal(message.MessageId, client.SentMessage.MessageId);
        Assert.Equal(1, client.DisconnectCalls);
    }

    /// <summary>
    /// Тест преобразования таймаута канала в ошибку доставки.
    /// </summary>
    /// <returns>Задача выполнения теста.</returns>
    [Fact]
    public async Task SendWithTimeoutTest()
    {
        var client = new TestSmtpClient { ConnectAction = token => Task.Delay(Timeout.Infinite, token) };
        var transport = new TestMailKitTransport(CreateSettings(), client);
        var error = await Assert.ThrowsAsync<MailDeliveryException>(() => transport.SendAsync(MailChannel.Primary, CreateMessage(), CancellationToken.None));

        Assert.Equal(Messages.SmtpTimeout, error.Message);
        Assert.IsAssignableFrom<OperationCanceledException>(error.InnerException);
    }

    /// <summary>
    /// Тест передачи отмены вызывающей стороны без преобразования в ошибку доставки.
    /// </summary>
    /// <returns>Задача выполнения теста.</returns>
    [Fact]
    public async Task SendWithCallerCancellationTest()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var client = new TestSmtpClient { ConnectAction = token => Task.FromCanceled(token) };
        var transport = new TestMailKitTransport(CreateSettings(), client);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => transport.SendAsync(MailChannel.Primary, CreateMessage(), cancellation.Token));
        Assert.Null(client.SentMessage);
    }

    /// <summary>
    /// Тест успешного результата при ошибке QUIT после принятия письма сервером.
    /// </summary>
    /// <returns>Задача выполнения теста.</returns>
    [Fact]
    public async Task SendWithDisconnectFailureTest()
    {
        var client = new TestSmtpClient { DisconnectError = new IOException(Messages.SmtpUnavailable) };
        var transport = new TestMailKitTransport(CreateSettings(), client);

        await transport.SendAsync(MailChannel.Primary, CreateMessage(), CancellationToken.None);

        Assert.NotNull(client.SentMessage);
        Assert.Equal(1, client.DisconnectCalls);
    }

    private static IOptions<MailSettings> CreateSettings()
    {
        return Options.Create(new MailSettings
        {
            Primary = new MailAccountSettings
            {
                SmtpHost = "primary.invalid",
                FromAddress = "primary@example.com",
                UserName = "primary@example.com",
                Password = "test",
                TimeoutSeconds = 1,
            },
            Fallback = new MailAccountSettings
            {
                SmtpHost = "fallback.invalid",
                FromAddress = "fallback@example.com",
                UserName = "fallback@example.com",
                Password = "test",
                TimeoutSeconds = 1,
            },
        });
    }

    private static OutgoingMailDto CreateMessage()
    {
        return new OutgoingMailDto
        {
            Recipient = "recipient@example.com",
            Subject = Messages.ProbeSubject,
            Body = Messages.ProbeBody,
            MessageId = "test@training-center.local",
        };
    }

    private class TestMailKitTransport : MailKitTransport
    {
        private readonly SmtpClient client;

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="settings">Настройки тестовых аккаунтов.</param>
        /// <param name="client">SMTP-клиент без сетевых подключений.</param>
        public TestMailKitTransport(IOptions<MailSettings> settings, SmtpClient client)
            : base(settings)
        {
            this.client = client;
        }

        /// <inheritdoc />
        protected override SmtpClient CreateSmtpClient() => this.client;
    }

    private class TestSmtpClient : SmtpClient
    {
        /// <summary>
        /// Действие этапа подключения.
        /// </summary>
        public Func<CancellationToken, Task>? ConnectAction { get; set; }

        /// <summary>
        /// Ошибка аутентификации.
        /// </summary>
        public Exception? AuthenticationError { get; set; }

        /// <summary>
        /// Ошибка отправки.
        /// </summary>
        public Exception? SendError { get; set; }

        /// <summary>
        /// Ошибка завершения соединения.
        /// </summary>
        public Exception? DisconnectError { get; set; }

        /// <summary>
        /// Выбранный сервер.
        /// </summary>
        public string? Host { get; private set; }

        /// <summary>
        /// Выбранный порт.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Выбранный способ подключения TLS.
        /// </summary>
        public SecureSocketOptions Security { get; private set; }

        /// <summary>
        /// Переданное клиенту письмо.
        /// </summary>
        public MimeMessage? SentMessage { get; private set; }

        /// <summary>
        /// Количество попыток завершения соединения.
        /// </summary>
        public int DisconnectCalls { get; private set; }

        /// <inheritdoc />
        public override Task ConnectAsync(string host, int port = 0, SecureSocketOptions options = SecureSocketOptions.Auto, CancellationToken cancellationToken = default)
        {
            this.Host = host;
            this.Port = port;
            this.Security = options;
            return this.ConnectAction?.Invoke(cancellationToken) ?? Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task AuthenticateAsync(Encoding encoding, ICredentials credentials, CancellationToken cancellationToken = default)
        {
            return this.AuthenticationError == null ? Task.CompletedTask : Task.FromException(this.AuthenticationError);
        }

        /// <inheritdoc />
        public override Task<string> SendAsync(FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default, ITransferProgress? progress = null)
        {
            if (this.SendError != null)
            {
                return Task.FromException<string>(this.SendError);
            }

            this.SentMessage = message;
            return Task.FromResult(string.Empty);
        }

        /// <inheritdoc />
        public override Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default)
        {
            this.DisconnectCalls++;
            return this.DisconnectError == null ? Task.CompletedTask : Task.FromException(this.DisconnectError);
        }
    }
}