using System.Collections.ObjectModel;

namespace PumpDrillingTelemetryCore.Domain.Models;

public class WorkSession
{
    public IReadOnlyCollection<Measure> Measures { get; }
    public IReadOnlyCollection<WorkInterval> WorkIntervals { get; }

    private WorkSession(IReadOnlyCollection<Measure> measures, IReadOnlyCollection<WorkInterval> intervals)
    {
        Measures = measures;
        WorkIntervals = intervals;
    }

    // Расчет единичного интервала для двух Measure
    private static WorkInterval CalculateWorkInterval(Measure firstMeasure, Measure secondMeasure)
    {  
        if (secondMeasure.TimeInSeconds - firstMeasure.TimeInSeconds > 1.5)
        {
            return new WorkInterval(secondMeasure.TimeInSeconds - firstMeasure.TimeInSeconds,
                WorkMode.Gapped);
        }
        else if (!firstMeasure.IsValid || firstMeasure.Rpm == null || firstMeasure.Rpm < 0 || 
            firstMeasure.Pressure == null || firstMeasure.Pressure < 0)
        {
            return new WorkInterval(secondMeasure.TimeInSeconds - firstMeasure.TimeInSeconds,
                WorkMode.Unreached);
        }
        else if (firstMeasure is { Rpm: >= 50, Pressure: >= 1.0 })
        {
            return firstMeasure.Pressure switch
            {
                < 5.00 => new WorkInterval(secondMeasure.TimeInSeconds - firstMeasure.TimeInSeconds, WorkMode.Low),
                < 10.0 => new WorkInterval(secondMeasure.TimeInSeconds - firstMeasure.TimeInSeconds, WorkMode.Normal),
                _ => new WorkInterval(secondMeasure.TimeInSeconds - firstMeasure.TimeInSeconds, WorkMode.High)
            };
        }
        else
        {
            return new WorkInterval(secondMeasure.TimeInSeconds - firstMeasure.TimeInSeconds,
                WorkMode.Stopped);
        }
    }

    // Логика расчёта WorkInterval исходя из Measures 
    public static WorkSession Create(Measure[] measures)
    {
        var intervals = new List<WorkInterval>();
        // Если в коллекции только один интервал, исходя из ожидаемого результата в ТЗ
        if (measures.Length < 2)
            return new WorkSession(measures, []);
        // Цикл проходится по всей коллекции measures и в соответствии с правилами из ТЗ вычисляется режим для каждого интервала
        for (var i = 0; i < measures.Length - 1; i++)
        {
            var measure = measures[i];
            var nextMeasure = measures[i + 1];
            var interval = CalculateWorkInterval(measure, nextMeasure);
            intervals.Add(interval);
        }
        return new WorkSession(measures, intervals);
    }

    public WorkSessionSummary GetAnalytics()
    {
        if (WorkIntervals.Count == 0)
        {
            var emptyDict = Enum.GetValues<WorkMode>()
                .ToDictionary(mode => mode, _ => 0.0);
            return new WorkSessionSummary(new ReadOnlyDictionary<WorkMode, double>(emptyDict), 0.0);
        }

        // Общее время сессии: разница между первой и последней меткой (по ТЗ)
        var totalSpan = Measures.Last().TimeInSeconds - Measures.First().TimeInSeconds;

        // Инициализируем словарь с ключами-режимами WorkMode и нулевыми значениями
        var durationByMode = Enum.GetValues<WorkMode>()
            .ToDictionary(mode => mode, _ => 0.0);
        // Считаем время для каждого режима
        foreach (var interval in WorkIntervals)
        {
            durationByMode[interval.WorkMode] += interval.Duration;
        }
        // Возвращаем запись WorkSessionSummary
        return new WorkSessionSummary(
            new ReadOnlyDictionary<WorkMode, double>(durationByMode),
            totalSpan
        );
    }
}
