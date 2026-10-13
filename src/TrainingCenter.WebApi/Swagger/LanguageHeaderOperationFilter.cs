namespace TrainingCenter.WebApi.Swagger;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TrainingCenter.Core.Resources;

/// <summary>
/// Добавляет выбор языка запроса в Swagger через заголовок Accept-Language.
/// </summary>
public class LanguageHeaderOperationFilter : IOperationFilter
{
    /// <summary>
    /// Добавляет необязательный заголовок с поддерживаемыми языками приложения.
    /// </summary>
    /// <param name="operation">Описание операции OpenAPI.</param>
    /// <param name="context">Контекст генерации описания операции.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Accept-Language",
            In = ParameterLocation.Header,
            Required = false,
            Description = Messages.LanguageHeaderDescription,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Enum = new List<IOpenApiAny> { new OpenApiString("ru"), new OpenApiString("en") },
                Default = new OpenApiString("ru"),
            },
        });
    }
}