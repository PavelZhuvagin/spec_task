using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TrainingCenter.BusinessLogic.Models.Mail;
using TrainingCenter.BusinessLogic.Services.Interfaces;
using TrainingCenter.BusinessLogic.Settings;
using TrainingCenter.DataAccess;
using TrainingCenter.DataAccess.Mail;
using TrainingCenter.Core.Resources;

// Отправляет только на собственный ящик, отдельно от рабочих заявок и имитации сбоя.
if (args.Length != 1 || !Enum.TryParse<MailChannel>(args[0], true, out var channel) || !Enum.IsDefined(channel))
{
    Console.Error.WriteLine(Messages.ProbeUsage);
    return 2;
}

try
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false)
        .AddUserSecrets("TrainingCenter-WebApi-local")
        .AddEnvironmentVariables()
        .Build();
    var services = new ServiceCollection().AddMail(configuration);
    services.AddSingleton<IMailInboxService>(provider => provider.GetRequiredService<MailKitTransport>());
    using var provider = services.BuildServiceProvider();

    var settings = provider.GetRequiredService<IOptions<MailSettings>>().Value;
    if (settings.Mode != MailMode.Live)
    {
        Console.Error.WriteLine(Messages.ProbeLiveRequired);
        return 2;
    }

    var account = settings.GetAccount(channel);
    var id = $"probe-{Guid.NewGuid():N}@training-center.local";
    var transport = provider.GetRequiredService<IMailTransport>();
    var inbox = provider.GetRequiredService<IMailInboxService>();
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(90));
    await transport.SendAsync(channel, new OutgoingMailDto
    {
        Recipient = account.FromAddress,
        Subject = Messages.ProbeSubject,
        Body = Messages.ProbeBody,
        MessageId = id,
    }, timeout.Token);
    Console.WriteLine(Messages.Format(Messages.ProbeSmtpAccepted, channel, id));
    for (var attempt = 0; attempt < 12; attempt++)
    {
        if (await inbox.ContainsAsync(channel, id, timeout.Token))
        {
            Console.WriteLine(Messages.ProbeSuccess);
            return 0;
        }

        await Task.Delay(TimeSpan.FromSeconds(3), timeout.Token);
    }

    Console.Error.WriteLine(Messages.ProbeNotFound);
    return 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine(Messages.Format(Messages.ProbeFailure, exception.GetType().Name));
    return 1;
}