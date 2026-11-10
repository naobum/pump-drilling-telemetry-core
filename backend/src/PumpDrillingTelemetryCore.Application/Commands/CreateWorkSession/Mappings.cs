using PumpDrillingTelemetryCore.Domain.Models;

namespace PumpDrillingTelemetryCore.Application.Commands.CreateWorkSession;

public static class Mappings
{
    public static WorkSessionSummaryResponse ToResponse(this WorkSession session)
    {
        var stoppedS = session.ModeDurations[WorkMode.Stopped];
        var gapS = session.ModeDurations[WorkMode.Gapped];
        var invalidS = session.ModeDurations[WorkMode.Unreached];

        var lowS = session.ModeDurations[WorkMode.Low];
        var normalS = session.ModeDurations[WorkMode.Normal];
        var highS = session.ModeDurations[WorkMode.High];

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
            SpanTime: session.Duration,
            ModesWorkTime: modesWorkTime
        );
    }
}