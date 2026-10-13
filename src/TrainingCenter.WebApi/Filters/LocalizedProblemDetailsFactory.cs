namespace TrainingCenter.WebApi.Filters;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TrainingCenter.Core.Resources;
using TrainingCenter.WebApi.Helpers;

/// <summary>
/// Создаёт локализованные ответы об ошибках с единым идентификатором трассировки.
/// </summary>
public class LocalizedProblemDetailsFactory : ProblemDetailsFactory
{
    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(HttpContext httpContext, int? statusCode = null, string? title = null, string? type = null, string? detail = null, string? instance = null)
    {
        var status = statusCode ?? StatusCodes.Status500InternalServerError;
        return new ProblemDetails
        {
            Status = status,
            Title = title ?? GetTitle(status),
            Type = type ?? "about:blank",
            Detail = detail,
            Instance = instance ?? httpContext.Request.Path,
            Extensions = { ["traceId"] = httpContext.GetTraceId() },
        };
    }

    /// <inheritdoc />
    public override ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext, ModelStateDictionary modelStateDictionary, int? statusCode = null, string? title = null, string? type = null, string? detail = null, string? instance = null)
    {
        var errors = modelStateDictionary.Where(entry => entry.Value?.Errors.Count > 0).ToDictionary(
            entry => entry.Key,
            entry => entry.Value!.Errors.Select(error => error.Exception != null || string.IsNullOrEmpty(error.ErrorMessage)
                ? Messages.ModelBindingError
                : error.ErrorMessage).ToArray());
        return new ValidationProblemDetails(errors)
        {
            Status = statusCode ?? StatusCodes.Status400BadRequest,
            Title = title ?? Messages.ModelValidationTitle,
            Type = type ?? "about:blank",
            Detail = detail,
            Instance = instance ?? httpContext.Request.Path,
            Extensions = { ["traceId"] = httpContext.GetTraceId() },
        };
    }

    private static string GetTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => Messages.InvalidRequest,
            StatusCodes.Status404NotFound => Messages.NotFound,
            StatusCodes.Status405MethodNotAllowed => Messages.MethodNotAllowed,
            StatusCodes.Status415UnsupportedMediaType => Messages.UnsupportedMediaType,
            StatusCodes.Status500InternalServerError => Messages.InternalServerError,
            StatusCodes.Status503ServiceUnavailable => Messages.DeliveryUnavailableTitle,
            _ => Messages.Format(Messages.HttpError, statusCode),
        };
    }
}