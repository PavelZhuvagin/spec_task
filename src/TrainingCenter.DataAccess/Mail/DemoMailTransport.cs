namespace TrainingCenter.DataAccess.Mail;

using System.Collections.Concurrent;
using TrainingCenter.BusinessLogic.Models.Mail;
using TrainingCenter.BusinessLogic.Services.Interfaces;

/// <summary>
/// Почтовый ящик в памяти для запуска без внешних аккаунтов.
/// </summary>
public class DemoMailTransport : IMailTransport
{
    private readonly ConcurrentQueue<(MailChannel Channel, OutgoingMailDto Message)> messages = new();

    /// <summary>
    /// Снимок писем, принятых демотранспортом, с указанием каналов.
    /// </summary>
    public IReadOnlyCollection<(MailChannel Channel, OutgoingMailDto Message)> Messages => this.messages.ToArray();

    /// <inheritdoc />
    public Task SendAsync(MailChannel channel, OutgoingMailDto message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        this.messages.Enqueue((channel, message));
        return Task.CompletedTask;
    }
}