using Microsoft.AspNetCore.Mvc;

namespace PumpDrillingTelemetryCore.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }

            // Отлавливаем исключение для JSON
            catch (System.Text.Json.JsonException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Некорректный формат JSON",
                    Detail = "Тело запроса содержит невалидный JSON, несоответствующие типы полей, " +
                    "либо не содержит обязательное поле.",
                    Instance = context.Request.Path
                };
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}
