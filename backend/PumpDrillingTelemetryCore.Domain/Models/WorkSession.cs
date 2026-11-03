using System.Collections.ObjectModel;

namespace PumpDrillingTelemetryCore.Domain.Models
{
    public class WorkSession
    {
        public IReadOnlyCollection<Measure> Measures { get; } = [];
        public IReadOnlyCollection<WorkInterval> WorkIntervals { get; } = [];

        private WorkSession(IReadOnlyCollection<Measure> measures, IReadOnlyCollection<WorkInterval> intervals)
        {
            Measures = measures;
            WorkIntervals = intervals;
        }

        // логика расчёта WorkInterval исходя из Measures 
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
                if (nextMeasure.TimeInSeconds - measure.TimeInSeconds > 1.5)
                {
                    intervals.Add(new WorkInterval(nextMeasure.TimeInSeconds - measure.TimeInSeconds,
                        WorkMode.Gapped));
                }
                else if (!measure.IsValid || measure.Rpm < 0 || measure.Pressure < 0)
                {
                    intervals.Add(new WorkInterval(nextMeasure.TimeInSeconds - measure.TimeInSeconds,
                        WorkMode.Unreached));
                }
                else if (measure is {Rpm: >= 50, Pressure: >= 1.0 })
                {
                    switch (measure.Pressure)
                    {
                        case < 5.00:
                            intervals.Add(new WorkInterval(nextMeasure.TimeInSeconds - measure.TimeInSeconds,
                                WorkMode.Low));
                            break;

                        case < 10.0:
                            intervals.Add(new WorkInterval(nextMeasure.TimeInSeconds - measure.TimeInSeconds,
                                WorkMode.Normal));
                            break;

                        default:
                            intervals.Add(new WorkInterval(nextMeasure.TimeInSeconds - measure.TimeInSeconds,
                                WorkMode.High));
                            break;
                    }
                }
                else
                {
                    intervals.Add(new WorkInterval(nextMeasure.TimeInSeconds - measure.TimeInSeconds,
                        WorkMode.Stopped));
                }
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
}
