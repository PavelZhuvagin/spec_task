namespace TrainingCenter.WebApi.Filters;

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using TrainingCenter.Core.Exceptions;
using TrainingCenter.Core.Resources;
using TrainingCenter.WebApi.Helpers;

/// <summary>
/// Преобразует исключения в безопасный ProblemDetails.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ExceptionHandlingMiddleware> logger;

    /// <summary>
    /// ctor.
    /// </summary>
    /// <param name="next">Следующий обработчик запроса.</param>
    /// <param name="logger">Логгер ошибок и отменённых запросов.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    /// <summary>
    /// Выполняет запрос и преобразует ошибки в ProblemDetails, если ответ ещё не начат.
    /// </summary>
    /// <param name="context">Контекст текущего HTTP-запроса.</param>
    /// <returns>Задача обработки запроса либо записи ответа об ошибке.</returns>
    /// <remarks>
    /// Недоступность доставки даёт 503, ошибка валидации 400, прочие ошибки 500. Отмена клиентом отмечается статусом 499 без записи тела ответа.
    /// </remarks>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this.next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            this.logger.LogInformation(Messages.RequestCancelledLog, context.GetTraceId());
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 499;
            }
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var (status, title) = exception switch
            {
                DeliveryUnavailableException => (StatusCodes.Status503ServiceUnavailable, Messages.DeliveryUnavailableTitle),
                ValidationException => (StatusCodes.Status400BadRequest, Messages.InvalidRequest),
                _ => (StatusCodes.Status500InternalServerError, Messages.InternalServerError),
            };
            var traceId = context.GetTraceId();
            this.logger.LogError(exception, Messages.RequestErrorLog, traceId, status);
            context.Response.Clear();
            context.Response.StatusCode = status;
            context.Response.Headers.ContentLanguage = CultureInfo.CurrentUICulture.Name;
            context.Response.Headers["X-Trace-Id"] = traceId;
            var factory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var problem = factory.CreateProblemDetails(context, statusCode: status, title: title);
            await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: context.RequestAborted);
        }
    }
}