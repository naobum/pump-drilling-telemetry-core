using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PumpDrillingTelemetryCore.Api.Requests.CreateWorkSession;

public record CreateWorkSessionRequest
{
    [JsonPropertyName("schema_version"), Required]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("dataset_id"), Required]
    public required string DatasetId { get; init; }

    [JsonPropertyName("synthetic")]
    public required bool Synthetic { get; init; }

    [JsonPropertyName("samples")]
    public MeasureDto[] Samples { get; init; } = [];
}

