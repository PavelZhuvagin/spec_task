namespace TrainingCenter.WebApi.Helpers;

using System.Diagnostics;

/// <summary>
/// Доступ к идентификатору трассировки текущего HTTP-запроса.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Возвращает TraceId текущей Activity либо идентификатор запроса, если Activity отсутствует.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    /// <returns>Идентификатор, общий для логов, заголовка и ответа об ошибке.</returns>
    public static string GetTraceId(this HttpContext context)
    {
        return Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    }
}