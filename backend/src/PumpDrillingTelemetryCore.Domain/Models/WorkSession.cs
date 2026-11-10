using System.Collections.ObjectModel;

namespace PumpDrillingTelemetryCore.Domain.Models;

public class WorkSession
{
    public IReadOnlyCollection<Measure> Measures { get; } = [];
    public IReadOnlyCollection<WorkInterval> WorkIntervals { get; } = [];
    public IReadOnlyDictionary<WorkMode, double> ModeDurations { get; private set; }
    public double Duration { get; private set; }

    private WorkSession(IReadOnlyCollection<Measure> measures, IReadOnlyCollection<WorkInterval> intervals)
    {
        Measures = measures;
        WorkIntervals = intervals;

        var emptyDict = Enum.GetValues<WorkMode>().ToDictionary(mode => mode, _ => 0.0);
        ModeDurations = new ReadOnlyDictionary<WorkMode, double>(emptyDict);
        Duration = 0.0;
    }

    private static WorkInterval CalculateWorkInterval(Measure firstMeasure, Measure secondMeasure)
    {  
        if (secondMeasure.TimeInSeconds - firstMeasure.TimeInSeconds > 1.5)
        {
            return new WorkInterval(secondMeasure.TimeInSeconds - firstMeasure.TimeInSeconds, WorkMode.Gapped);
        }
        
        if (!firstMeasure.IsValid || firstMeasure.Rpm == null || firstMeasure.Rpm < 0 || 
            firstMeasure.Pressure == null || firstMeasure.Pressure < 0)
        {
            return new WorkInterval(secondMeasure.TimeInSeconds - firstMeasure.TimeInSeconds, WorkMode.Unreached);
        }
        
        if (firstMeasure is { Rpm: >= 50, Pressure: >= 1.0 })
        {
            return firstMeasure.Pressure switch
            {
                < 5.00 => new WorkInterval(secondMeasure.TimeInSeconds - firstMeasure.TimeInSeconds, WorkMode.Low),
                < 10.0 => new WorkInterval(secondMeasure.TimeInSeconds - firstMeasure.TimeInSeconds, WorkMode.Normal),
                _ => new WorkInterval(secondMeasure.TimeInSeconds - firstMeasure.TimeInSeconds, WorkMode.High)
            };
        }

        return new WorkInterval(secondMeasure.TimeInSeconds - firstMeasure.TimeInSeconds, WorkMode.Stopped);
    }

    public static WorkSession Create(Measure[] measures)
    {
        var intervals = new List<WorkInterval>();
        if (measures.Length < 2)
            return new WorkSession(measures, []);

        for (var i = 0; i < measures.Length - 1; i++)
        {
            var measure = measures[i];
            var nextMeasure = measures[i + 1];
            intervals.Add(CalculateWorkInterval(measure, nextMeasure));
        }

        return new WorkSession(measures, intervals);
    }

    public void CalculateAnalytics()
    {
        var durationByMode = Enum.GetValues<WorkMode>()
            .ToDictionary(mode => mode, _ => 0.0);

        if (Measures.Count < 2 || WorkIntervals.Count == 0)
        {
            ModeDurations = new ReadOnlyDictionary<WorkMode, double>(durationByMode);
            Duration = 0.0;
            return;
        }

        Duration = Measures.Last().TimeInSeconds - Measures.First().TimeInSeconds;

        foreach (var interval in WorkIntervals)
        {
            durationByMode[interval.WorkMode] += interval.Duration;
        }

        ModeDurations = new ReadOnlyDictionary<WorkMode, double>(durationByMode);
    }
}