using Microsoft.AspNetCore.Mvc.Infrastructure;
using TrainingCenter.WebApi;
using TrainingCenter.WebApi.Filters;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders().AddConsole();
builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

app.UseRequestLocalization();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStatusCodePages(async statusContext =>
{
    var context = statusContext.HttpContext;
    var factory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
    var problem = factory.CreateProblemDetails(context, statusCode: context.Response.StatusCode);
    await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: context.RequestAborted);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();

/// <summary>
/// Точка входа Web API
/// </summary>
public partial class Program;