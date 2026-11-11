using System.Text.Json.Serialization;
using PumpDrillingTelemetryCore.Domain.Models;

namespace PumpDrillingTelemetryCore.Application.Commands.CreateWorkSession;

public record WorkSessionDetailsResponse(
    [property: JsonPropertyName("dataset_id")] string DatasetId,
    [property: JsonPropertyName("summary")] WorkSessionSummaryResponse Summary,
    [property: JsonPropertyName("samples")] IReadOnlyCollection<Measure> Samples
);