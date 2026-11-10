namespace PumpDrillingTelemetryCore.Domain.Models;  

public record Measure(double TimeInSeconds, double? Pressure, int? Rpm, bool IsValid);

