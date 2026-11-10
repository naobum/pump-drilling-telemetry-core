namespace PumpDrillingTelemetryCore.Domain.Models;  

public record Measure(double TimeInSeconds, double? Pressure, double? Rpm, bool IsValid);

