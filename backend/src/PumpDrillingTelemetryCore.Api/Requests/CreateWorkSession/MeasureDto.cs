using System.Text.Json.Serialization;

namespace PumpDrillingTelemetryCore.Api.Requests.CreateWorkSession;

public record MeasureDto
{
    [JsonPropertyName("t_s")]
    public required double TimeInSeconds { get; init; }

    [JsonPropertyName("pressure_mpa")]
    public required double? Pressure { get; init; }

    [JsonPropertyName("rpm")]
    public required double? Rpm { get; init; }

    [JsonPropertyName("valid")]
    public required bool IsValid { get; init; }
}