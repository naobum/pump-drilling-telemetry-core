using PumpDrillingTelemetryCore.Domain.Models;

namespace PumpDrillingTelemetryCore.Application.Commands.CreateWorkSession;

public static class Mappings
{
    public static WorkSessionSummaryResponse ToResponse(this WorkSessionSummary summary)
    {
        var stoppedS = summary.ModeDurations[WorkMode.Stopped];
        var gapS = summary.ModeDurations[WorkMode.Gapped];
        var invalidS = summary.ModeDurations[WorkMode.Unreached];

        var lowS = summary.ModeDurations[WorkMode.Low];
        var normalS = summary.ModeDurations[WorkMode.Normal];
        var highS = summary.ModeDurations[WorkMode.High];

        var workingS = lowS + normalS + highS;
        var unknownS = gapS + invalidS;

        var modesWorkTime = new Dictionary<string, double>
        {
            ["low"] = lowS,
            ["normal"] = normalS,
            ["high"] = highS
        };

        return new WorkSessionSummaryResponse(
            WorkingTime: workingS,
            StoppedTime: stoppedS,
            GappedTime: gapS,
            InvalidTime: invalidS,
            UnknownTime: unknownS,
            SpanTime: summary.Duration,
            ModesWorkTime: modesWorkTime
        );
    }
}