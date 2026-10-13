namespace TrainingCenter.BusinessLogic.Settings;

using TrainingCenter.Core.Resources;

/// <summary>
/// Настройки почтовых каналов, имитации сбоя и маршрутизации уведомлений.
/// </summary>
public class MailSettings
{
    /// <summary>
    /// Имя секции почтовой конфигурации.
    /// </summary>
    public const string SectionName = "Mail";

    /// <summary>
    /// Режим работы транспорта: память либо реальные почтовые серверы.
    /// </summary>
    public MailMode Mode { get; set; } = MailMode.Demo;

    /// <summary>
    /// Признак принудительного сбоя основного канала для проверки резерва.
    /// </summary>
    public bool SimulatePrimaryFailure { get; set; } = true;

    /// <summary>
    /// Адрес получателя уведомлений основного канала.
    /// </summary>
    public string PrimaryRecipient { get; set; } = "training-center@yandex.ru";

    /// <summary>
    /// Адрес резервного получателя заявок юридических лиц.
    /// </summary>
    public string LegalEntityRecipient { get; set; } = "mail1@yandex.ru";

    /// <summary>
    /// Адрес резервного получателя заявок физических лиц.
    /// </summary>
    public string IndividualRecipient { get; set; } = "mail2@yandex.ru";

    /// <summary>
    /// Настройки основного почтового аккаунта.
    /// </summary>
    public MailAccountSettings Primary { get; set; } = new();

    /// <summary>
    /// Настройки резервного почтового аккаунта.
    /// </summary>
    public MailAccountSettings Fallback { get; set; } = new();


    /// <summary>
    /// Возвращает настройки аккаунта выбранного канала.
    /// </summary>
    /// <param name="channel">Основной либо резервный канал.</param>
    /// <returns>Настройки подключения к SMTP и IMAP.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Канал не поддерживается.</exception>
    public MailAccountSettings GetAccount(Models.Mail.MailChannel channel) => channel switch
    {
        Models.Mail.MailChannel.Primary => this.Primary,
        Models.Mail.MailChannel.Fallback => this.Fallback,
        _ => throw new ArgumentOutOfRangeException(nameof(channel), Messages.UnsupportedChannel),
    };
}

/// <summary>
/// Способ передачи писем почтовым транспортом.
/// </summary>
public enum MailMode
{
    /// <summary>
    /// Письма сохраняются в коллекции в памяти без сетевых подключений.
    /// </summary>
    Demo,
    /// <summary>
    /// Письма отправляются через SMTP и проверяются через IMAP.
    /// </summary>
    Live,
}

/// <summary>
/// Параметры SMTP и IMAP одного почтового аккаунта.
/// </summary>
public class MailAccountSettings
{
    /// <summary>
    /// Имя SMTP-сервера.
    /// </summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// Порт SMTP: 465 для TLS при соединении либо 587 для STARTTLS.
    /// </summary>
    public int SmtpPort { get; set; } = 465;

    /// <summary>
    /// Имя IMAP-сервера.
    /// </summary>
    public string ImapHost { get; set; } = string.Empty;

    /// <summary>
    /// Порт IMAP с TLS при соединении; поддерживается 993.
    /// </summary>
    public int ImapPort { get; set; } = 993;

    /// <summary>
    /// Логин почтового аккаунта для аутентификации SMTP и IMAP.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Пароль приложения; задаётся через User Secrets либо переменные окружения.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Разрешённый адрес отправителя; также получатель письма утилиты проверки.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Таймаут операции почтового канала в секундах, от 1 до 120.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 15;
}