namespace TrainingCenter.DataAccess.Settings;

using Microsoft.Extensions.Options;
using MimeKit;
using TrainingCenter.BusinessLogic.Settings;
using TrainingCenter.Core.Resources;

/// <summary>
/// Проверяет режим почты, адреса получателей и параметры аккаунтов в режиме Live.
/// </summary>
public class MailSettingsValidator : IValidateOptions<MailSettings>
{
    /// <summary>
    /// Проверяет почтовую конфигурацию перед использованием сервисов.
    /// </summary>
    /// <param name="name">Имя проверяемого набора настроек.</param>
    /// <param name="options">Параметры почтовых каналов.</param>
    /// <returns>Успешный результат либо перечень ошибок конфигурации.</returns>
    public ValidateOptionsResult Validate(string? name, MailSettings options)
    {
        var errors = new List<string>();
        if (!Enum.IsDefined(options.Mode))
        {
            errors.Add(Messages.InvalidMailMode);
        }

        foreach (var address in new[] { options.PrimaryRecipient, options.LegalEntityRecipient, options.IndividualRecipient })
        {
            if (!MailboxAddress.TryParse(address, out _))
            {
                errors.Add(Messages.InvalidRecipient);
            }
        }

        if (options.Mode == MailMode.Live)
        {
            ValidateAccount(options.Primary, "Primary", errors);
            ValidateAccount(options.Fallback, "Fallback", errors);
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }

    private static void ValidateAccount(MailAccountSettings account, string channel, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(account.SmtpHost) || string.IsNullOrWhiteSpace(account.ImapHost)
            || string.IsNullOrWhiteSpace(account.UserName) || string.IsNullOrWhiteSpace(account.Password)
            || !MailboxAddress.TryParse(account.FromAddress, out _))
        {
            errors.Add(Messages.Format(Messages.IncompleteAccount, channel));
        }

        if (account.SmtpPort is not (465 or 587) || account.ImapPort != 993 || account.TimeoutSeconds is < 1 or > 120)
        {
            errors.Add(Messages.Format(Messages.InvalidAccountConnection, channel));
        }
    }
}