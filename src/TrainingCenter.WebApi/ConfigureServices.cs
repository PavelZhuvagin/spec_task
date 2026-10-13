namespace TrainingCenter.WebApi;

using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using TrainingCenter.BusinessLogic;
using TrainingCenter.DataAccess;
using TrainingCenter.Core.Resources;
using TrainingCenter.WebApi.Filters;
using TrainingCenter.WebApi.Swagger;

/// <summary>
/// Регистрация зависимостей и инфраструктуры Web API.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Регистрирует слои приложения, MVC, строковую сериализацию enum и Swagger.
    /// </summary>
    /// <param name="services">Коллекция зависимостей приложения.</param>
    /// <param name="configuration">Конфигурация приложения и почтовых каналов.</param>
    /// <returns>Коллекция с зависимостями Web API.</returns>
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBusinessLogic().AddDataAccess(configuration);
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.SetDefaultCulture("ru").AddSupportedCultures("ru", "en").AddSupportedUICultures("ru", "en");
            options.RequestCultureProviders = new[] { new AcceptLanguageHeaderRequestCultureProvider() };
            options.ApplyCurrentCultureToResponseHeaders = true;
        });
        services.AddControllers(options =>
        {
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            var messages = options.ModelBindingMessageProvider;
            messages.SetMissingRequestBodyRequiredValueAccessor(() => Messages.ModelBindingError);
        }).AddJsonOptions(options =>
        {
            options.AllowInputFormatterExceptionMessages = false;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        });
        services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = context =>
        {
            var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var problem = factory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);
            return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
        });
        services.AddTransient<ProblemDetailsFactory, LocalizedProblemDetailsFactory>();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = Messages.SwaggerTitle, Version = "v1" });
            options.OperationFilter<LanguageHeaderOperationFilter>();
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
        });
        return services;
    }
}