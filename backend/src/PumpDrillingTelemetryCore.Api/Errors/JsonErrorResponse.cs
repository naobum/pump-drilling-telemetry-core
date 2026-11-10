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
            Title = "Некорректный JSON",
            Detail = exception.Message,
            Instance = instance,
            Extensions =
            {
                ["path"] = path,
                ["lineNumber"] = exception.LineNumber,
                ["bytePositionInLine"] = exception.BytePositionInLine
            }
        };
        return problem;
    }
}
