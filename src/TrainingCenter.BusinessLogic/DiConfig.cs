namespace TrainingCenter.BusinessLogic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TrainingCenter.BusinessLogic.Services;
using TrainingCenter.BusinessLogic.Services.Interfaces;

/// <summary>
/// Регистрация зависимостей бизнес-логики.
/// </summary>
public static class DiConfig
{
    /// <summary>
    /// Регистрирует сервисы корзины, заявок и системный источник времени.
    /// </summary>
    /// <param name="services">Коллекция зависимостей приложения.</param>
    /// <returns>Коллекция с зарегистрированными сервисами.</returns>
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        return services
            .AddScoped<ICartService, CartService>()
            .AddScoped<ILeadService, LeadService>();
    }
}