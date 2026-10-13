namespace TrainingCenter.DataAccess.Mail;

using System.Net.Sockets;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TrainingCenter.BusinessLogic.Models.Mail;
using TrainingCenter.BusinessLogic.Services.Interfaces;
using TrainingCenter.BusinessLogic.Settings;
using TrainingCenter.Core.Exceptions;
using TrainingCenter.Core.Resources;
using IMailTransport = TrainingCenter.BusinessLogic.Services.Interfaces.IMailTransport;

/// <summary>
/// SMTP-отправка и IMAP-проверка входящих писем через TLS.
/// </summary>
public class MailKitTransport : IMailTransport, IMailInboxService
{
    private readonly MailSettings settings;

    /// <summary>
    /// ctor.
    /// </summary>
    /// <param name="settings">Настройки основного и резервного почтовых аккаунтов.</param>
    public MailKitTransport(IOptions<MailSettings> settings)
    {
        this.settings = settings.Value;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Использует TLS при соединении на порту 465 либо обязательный STARTTLS на порту 587. Таймаут канала преобразуется в ошибку доставки; отмена вызывающей стороны передаётся без преобразования.
    /// </remarks>
    public async Task SendAsync(MailChannel channel, OutgoingMailDto message, CancellationToken cancellationToken)
    {
        var account = this.settings.GetAccount(channel);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(account.TimeoutSeconds));
        using var client = this.CreateSmtpClient();
        client.Timeout = account.TimeoutSeconds * 1000;
        var mimeMessage = new MimeMessage
        {
            Subject = message.Subject,
            MessageId = message.MessageId,
            Body = new TextPart("plain") { Text = message.Body },
        };
        mimeMessage.From.Add(MailboxAddress.Parse(account.FromAddress));
        mimeMessage.To.Add(MailboxAddress.Parse(message.Recipient));

        try
        {
            var tls = account.SmtpPort == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            await client.ConnectAsync(account.SmtpHost, account.SmtpPort, tls, timeout.Token);
            await client.AuthenticateAsync(account.UserName, account.Password, timeout.Token);
            await client.SendAsync(mimeMessage, timeout.Token);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new MailDeliveryException(Messages.SmtpTimeout, exception);
        }
        catch (Exception exception) when (exception is IOException or SocketException or MailKit.Security.AuthenticationException or System.Security.Authentication.AuthenticationException or CommandException or ProtocolException)
        {
            throw new MailDeliveryException(Messages.SmtpUnavailable, exception);
        }

        try
        {
            await client.DisconnectAsync(true, timeout.Token);
        }
        catch (Exception exception) when (exception is IOException or SocketException or ProtocolException or OperationCanceledException)
        {
            // SMTP уже подтвердил принятие письма. Ошибка QUIT не должна запускать резерв и дублировать отправку.
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Открывает INBOX только для чтения и выполняет поиск заголовка Message-Id. Ошибки IMAP и отмена передаются вызывающей стороне.
    /// </remarks>
    public async Task<bool> ContainsAsync(MailChannel channel, string messageId, CancellationToken cancellationToken)
    {
        var account = this.settings.GetAccount(channel);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(account.TimeoutSeconds));
        using var client = new ImapClient { Timeout = account.TimeoutSeconds * 1000 };
        await client.ConnectAsync(account.ImapHost, account.ImapPort, SecureSocketOptions.SslOnConnect, timeout.Token);
        await client.AuthenticateAsync(account.UserName, account.Password, timeout.Token);
        await client.Inbox.OpenAsync(FolderAccess.ReadOnly, timeout.Token);
        var ids = await client.Inbox.SearchAsync(SearchQuery.HeaderContains("Message-Id", messageId), timeout.Token);
        await client.DisconnectAsync(true, timeout.Token);
        return ids.Count > 0;
    }

    /// <summary>
    /// Создаёт отдельный SMTP-клиент для одной операции отправки.
    /// </summary>
    /// <returns>Клиент, который будет освобождён после завершения операции.</returns>
    protected virtual SmtpClient CreateSmtpClient()
    {
        return new SmtpClient();
    }
}