using System.Text.Json.Serialization;

namespace PumpDrillingTelemetryCore.Application.Commands.CreateWorkSession;

public record WorkSessionSummaryResponse(
    [property: JsonPropertyName("working_s")] double WorkingTime,
    [property: JsonPropertyName("stopped_s")] double StoppedTime,
    [property: JsonPropertyName("gap_s")] double GappedTime,
    [property: JsonPropertyName("invalid_s")] double InvalidTime,
    [property: JsonPropertyName("unknown_s")] double UnknownTime,
    [property: JsonPropertyName("span_s")] double SpanTime,
    [property: JsonPropertyName("modes_s")] Dictionary<string, double> ModesWorkTime
);