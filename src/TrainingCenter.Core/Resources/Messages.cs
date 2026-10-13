namespace TrainingCenter.Core.Resources;

using System.Globalization;
using System.Resources;

/// <summary>
/// Сообщения приложения на русском и английском языках из встроенных ресурсов Core.
/// </summary>
public static class Messages
{
    private static readonly ResourceManager Manager = new("TrainingCenter.Core.Resources.Messages", typeof(Messages).Assembly);

    /// <summary>
    /// Заявка принята.
    /// </summary>
    public static string LeadAccepted => Manager.GetString(nameof(LeadAccepted), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Заявка сохранена, но почтовые каналы временно недоступны.
    /// </summary>
    public static string DeliveryUnavailable => Manager.GetString(nameof(DeliveryUnavailable), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Заявка сохранена, но доставка уведомления временно недоступна.
    /// </summary>
    public static string DeliveryUnavailableTitle => Manager.GetString(nameof(DeliveryUnavailableTitle), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Некорректные данные запроса.
    /// </summary>
    public static string InvalidRequest => Manager.GetString(nameof(InvalidRequest), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Внутренняя ошибка сервера.
    /// </summary>
    public static string InternalServerError => Manager.GetString(nameof(InternalServerError), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Имитируется недоступность основного почтового сервиса.
    /// </summary>
    public static string PrimaryFailureSimulation => Manager.GetString(nameof(PrimaryFailureSimulation), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Истекло время ожидания SMTP.
    /// </summary>
    public static string SmtpTimeout => Manager.GetString(nameof(SmtpTimeout), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// SMTP-сервис недоступен.
    /// </summary>
    public static string SmtpUnavailable => Manager.GetString(nameof(SmtpUnavailable), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Основной почтовый канал недоступен. Заявка {LeadId} отправляется через резервный канал.
    /// </summary>
    public static string PrimaryFallbackLog => Manager.GetString(nameof(PrimaryFallbackLog), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Клиент отменил запрос {TraceId}.
    /// </summary>
    public static string RequestCancelledLog => Manager.GetString(nameof(RequestCancelledLog), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Ошибка обработки запроса {TraceId}; StatusCode={StatusCode}.
    /// </summary>
    public static string RequestErrorLog => Manager.GetString(nameof(RequestErrorLog), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// HTTP-запрос {Method} {Path}; ContentLength={ContentLength}.
    /// </summary>
    public static string RequestLog => Manager.GetString(nameof(RequestLog), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// HTTP-ответ {StatusCode}; ContentType={ContentType}; ContentLength={ContentLength}; ElapsedMs={ElapsedMs}; Aborted={Aborted}.
    /// </summary>
    public static string ResponseLog => Manager.GetString(nameof(ResponseLog), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// TraceId:{TraceId}.
    /// </summary>
    public static string TraceScope => Manager.GetString(nameof(TraceScope), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Mail:Mode должен быть Demo или Live.
    /// </summary>
    public static string InvalidMailMode => Manager.GetString(nameof(InvalidMailMode), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Mail: адрес получателя некорректен.
    /// </summary>
    public static string InvalidRecipient => Manager.GetString(nameof(InvalidRecipient), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Mail:{0}: заполните серверы, UserName, Password и FromAddress.
    /// </summary>
    public static string IncompleteAccount => Manager.GetString(nameof(IncompleteAccount), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Mail:{0}: SMTP 465/587, IMAP 993, TimeoutSeconds от 1 до 120.
    /// </summary>
    public static string InvalidAccountConnection => Manager.GetString(nameof(InvalidAccountConnection), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Заявка на обратный звонок: {0}.
    /// </summary>
    public static string LeadSubject => Manager.GetString(nameof(LeadSubject), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Имя: {0} Телефон: {1} Email: {2} Тип клиента: {3} Номер заявки: {4}.
    /// </summary>
    public static string LeadBody => Manager.GetString(nameof(LeadBody), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Физическое лицо.
    /// </summary>
    public static string IndividualClient => Manager.GetString(nameof(IndividualClient), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Юридическое лицо.
    /// </summary>
    public static string LegalEntityClient => Manager.GetString(nameof(LegalEntityClient), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Поле «{0}» обязательно.
    /// </summary>
    public static string RequiredField => Manager.GetString(nameof(RequiredField), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Поле «{0}» должно содержать не более {1} символов.
    /// </summary>
    public static string StringLength => Manager.GetString(nameof(StringLength), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Поле «{0}» должно содержать корректный телефон.
    /// </summary>
    public static string InvalidPhone => Manager.GetString(nameof(InvalidPhone), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Поле «{0}» должно содержать корректный email.
    /// </summary>
    public static string InvalidEmail => Manager.GetString(nameof(InvalidEmail), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Поле «{0}» должно содержать поддерживаемый тип клиента.
    /// </summary>
    public static string InvalidClientType => Manager.GetString(nameof(InvalidClientType), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Имя.
    /// </summary>
    public static string NameField => Manager.GetString(nameof(NameField), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Телефон.
    /// </summary>
    public static string PhoneField => Manager.GetString(nameof(PhoneField), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Email.
    /// </summary>
    public static string EmailField => Manager.GetString(nameof(EmailField), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Тип клиента.
    /// </summary>
    public static string ClientTypeField => Manager.GetString(nameof(ClientTypeField), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Один или несколько параметров запроса некорректны.
    /// </summary>
    public static string ModelValidationTitle => Manager.GetString(nameof(ModelValidationTitle), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Значение поля некорректно или обязательное тело запроса отсутствует.
    /// </summary>
    public static string ModelBindingError => Manager.GetString(nameof(ModelBindingError), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Список курсов обязателен.
    /// </summary>
    public static string CoursesRequired => Manager.GetString(nameof(CoursesRequired), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Профиль пользователя обязателен.
    /// </summary>
    public static string UserRequired => Manager.GetString(nameof(UserRequired), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Список курсов не должен содержать пустых элементов.
    /// </summary>
    public static string CourseRequired => Manager.GetString(nameof(CourseRequired), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Цена курса не может быть отрицательной.
    /// </summary>
    public static string NegativePrice => Manager.GetString(nameof(NegativePrice), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Почтовый канал не поддерживается.
    /// </summary>
    public static string UnsupportedChannel => Manager.GetString(nameof(UnsupportedChannel), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Тип клиента не поддерживается.
    /// </summary>
    public static string UnsupportedClient => Manager.GetString(nameof(UnsupportedClient), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Использование: dotnet run --project TrainingCenter.MailProbe -- Primary|Fallback.
    /// </summary>
    public static string ProbeUsage => Manager.GetString(nameof(ProbeUsage), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Для проверки реальной почты задайте Mail:Mode=Live и настройки обоих аккаунтов.
    /// </summary>
    public static string ProbeLiveRequired => Manager.GetString(nameof(ProbeLiveRequired), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Проверка SMTP/IMAP учебного центра.
    /// </summary>
    public static string ProbeSubject => Manager.GetString(nameof(ProbeSubject), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Проверка SMTP и получения письма через IMAP.
    /// </summary>
    public static string ProbeBody => Manager.GetString(nameof(ProbeBody), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// SMTP принял письмо. Канал: {0}; Message-Id: {1}.
    /// </summary>
    public static string ProbeSmtpAccepted => Manager.GetString(nameof(ProbeSmtpAccepted), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Письмо найдено в INBOX через IMAP. Проверка успешна.
    /// </summary>
    public static string ProbeSuccess => Manager.GetString(nameof(ProbeSuccess), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// SMTP принял письмо, но в INBOX оно пока не найдено. Проверьте спам и повторите проверку.
    /// </summary>
    public static string ProbeNotFound => Manager.GetString(nameof(ProbeNotFound), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Проверка не пройдена: {0}. Проверьте настройки аккаунта и доступность серверов.
    /// </summary>
    public static string ProbeFailure => Manager.GetString(nameof(ProbeFailure), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Учебный центр.
    /// </summary>
    public static string SwaggerTitle => Manager.GetString(nameof(SwaggerTitle), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Язык ответов API и почтовых уведомлений.
    /// </summary>
    public static string LanguageHeaderDescription => Manager.GetString(nameof(LanguageHeaderDescription), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Запрошенный ресурс не найден.
    /// </summary>
    public static string NotFound => Manager.GetString(nameof(NotFound), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// HTTP-метод не поддерживается для этого ресурса.
    /// </summary>
    public static string MethodNotAllowed => Manager.GetString(nameof(MethodNotAllowed), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Тип содержимого запроса не поддерживается.
    /// </summary>
    public static string UnsupportedMediaType => Manager.GetString(nameof(UnsupportedMediaType), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Ошибка HTTP с указанным статусом.
    /// </summary>
    public static string HttpError => Manager.GetString(nameof(HttpError), CultureInfo.CurrentUICulture)!;

    /// <summary>
    /// Подставляет параметры в локализованный шаблон сообщения.
    /// </summary>
    /// <param name="template">Шаблон с позиционными параметрами.</param>
    /// <param name="arguments">Значения параметров шаблона.</param>
    /// <returns>Сообщение, отформатированное с текущей культурой.</returns>
    public static string Format(string template, params object?[] arguments) => string.Format(CultureInfo.CurrentCulture, template, arguments);
}