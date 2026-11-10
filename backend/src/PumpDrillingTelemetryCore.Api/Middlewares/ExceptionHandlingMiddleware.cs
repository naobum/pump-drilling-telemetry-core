using PumpDrillingTelemetryCore.Api.Errors;

namespace PumpDrillingTelemetryCore.Api.Middlewares
{
    public class ExceptionHandlingMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }

            // Обработка исключений десериализации JSON
            catch (System.Text.Json.JsonException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                var problemDetails = JsonErrorResponse.Create(ex, context.Request.Path);
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}

