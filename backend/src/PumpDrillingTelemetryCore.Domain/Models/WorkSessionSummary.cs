namespace PumpDrillingTelemetryCore.Domain.Models;

public record WorkSessionSummary(IReadOnlyDictionary<WorkMode, double> ModeDurations, double Duration);
