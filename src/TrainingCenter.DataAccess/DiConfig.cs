namespace TrainingCenter.DataAccess;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TrainingCenter.BusinessLogic.Services.Interfaces;
using TrainingCenter.BusinessLogic.Settings;
using TrainingCenter.Core.Storage;
using TrainingCenter.DataAccess.Mail;
using TrainingCenter.DataAccess.Settings;

/// <summary>
/// Регистрация хранилища заявок и почтовой инфраструктуры.
/// </summary>
public static class DiConfig
{
    /// <summary>
    /// Регистрирует почтовые настройки, их проверку и транспорт отправки выбранного режима.
    /// </summary>
    /// <param name="services">Коллекция зависимостей приложения.</param>
    /// <param name="configuration">Конфигурация с секцией Mail.</param>
    /// <returns>Коллекция с сервисами отправки писем.</returns>
    public static IServiceCollection AddMail(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<MailSettings>, MailSettingsValidator>();
        services.AddOptions<MailSettings>()
            .Bind(configuration.GetSection(MailSettings.SectionName))
            .ValidateOnStart();
        services.AddSingleton<DemoMailTransport>();
        services.AddSingleton<MailKitTransport>();
        services.AddSingleton<IMailTransport>(provider =>
            provider.GetRequiredService<IOptions<MailSettings>>().Value.Mode == MailMode.Live
                ? provider.GetRequiredService<MailKitTransport>()
                : provider.GetRequiredService<DemoMailTransport>());
        return services;
    }

    /// <summary>
    /// Регистрирует EF Core InMemory, единицу работы с generic-репозиториями и почтовые сервисы.
    /// </summary>
    /// <param name="services">Коллекция зависимостей приложения.</param>
    /// <param name="configuration">Конфигурация почтовых каналов.</param>
    /// <returns>Коллекция с зависимостями слоя доступа к данным.</returns>
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TrainingCenterDbContext>(options => options.UseInMemoryDatabase("TrainingCenter"));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services.AddMail(configuration);
    }
}