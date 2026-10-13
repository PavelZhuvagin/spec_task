namespace TrainingCenter.WebApi.Filters;

using System.Diagnostics;
using TrainingCenter.Core.Resources;
using TrainingCenter.WebApi.Helpers;

/// <summary>
/// Логирует запрос и итоговый ответ с общим TraceId.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<RequestLoggingMiddleware> logger;

    /// <summary>
    /// ctor.
    /// </summary>
    /// <param name="next">Следующий обработчик запроса.</param>
    /// <param name="logger">Логгер метаданных запроса и результата.</param>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    /// <summary>
    /// Логирует метаданные запроса, итоговый статус и длительность с общим TraceId.
    /// </summary>
    /// <param name="context">Контекст текущего HTTP-запроса.</param>
    /// <returns>Задача выполнения следующего обработчика и логирования результата.</returns>
    /// <remarks>
    /// Тела, query string и заголовки авторизации не логируются; TraceId возвращается в X-Trace-Id.
    /// </remarks>
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var traceId = context.GetTraceId();
        using var scope = this.logger.BeginScope(Messages.TraceScope, traceId);
        context.Response.Headers["X-Trace-Id"] = traceId;

        this.logger.LogInformation(Messages.RequestLog,
            context.Request.Method, context.Request.Path, context.Request.ContentLength);
        try
        {
            await this.next(context);
        }
        finally
        {
            this.logger.LogInformation(Messages.ResponseLog,
                context.Response.StatusCode, context.Response.ContentType, context.Response.ContentLength,
                stopwatch.Elapsed.TotalMilliseconds, context.RequestAborted.IsCancellationRequested);
        }
    }
}