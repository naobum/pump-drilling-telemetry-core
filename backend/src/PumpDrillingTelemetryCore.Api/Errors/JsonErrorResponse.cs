using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace PumpDrillingTelemetryCore.Api.Errors;

internal static class JsonErrorResponse
{
    public static ValidationProblemDetails Create(JsonException exception, string instance)
    {
        var path = exception.Path ?? "$";
        var problem = new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            [path] = [exception.Message]
        })
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Некорректный формат JSON",
            Detail = exception.Message,
            Instance = instance
        };
        problem.Extensions["path"] = path;
        problem.Extensions["lineNumber"] = exception.LineNumber;
        problem.Extensions["bytePositionInLine"] = exception.BytePositionInLine;
        return problem;
    }
}
