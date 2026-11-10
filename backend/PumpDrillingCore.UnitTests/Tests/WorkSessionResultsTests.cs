using System.Text.Json;
using System.Text.Json.Serialization;
using PumpDrillingTelemetryCore.Domain.Models;

namespace PumpDrillingCore.UnitTests.tests
{
    public class WorkSessionResultsTests
    {
        // Класс-представление для парсинга из json
        private record PumpMeasurementSample
        {
            [JsonPropertyName("t_s")]
            public double TimeSecond { get; init; }
            [JsonPropertyName("pressure_mpa")]
            public double? Pressure { get; init; }
            [JsonPropertyName("rpm")]
            public double? Rpm { get; init; }
            [JsonPropertyName("valid")]
            public bool IsValid { get; init; }
        }

        [Theory]
        [InlineData("01_small.json")]
        [InlineData("02_boundaries.json")]
        [InlineData("03_gap_20min.json")]
        [InlineData("04_invalid_values.json")]
        [InlineData("05_long_record.json")]
        [InlineData("06_empty.json")]
        [InlineData("07_single_point.json")]
        public void ProcessSession_ValidFiles_MatchesExpectedResults(string fileName)
        {
            // [Arrange]
            var basePath = AppContext.BaseDirectory;
            var filePath = Path.Combine(basePath, "testData", fileName);
            var expectedPath = Path.Combine(basePath, "testData", "expected_results.json");

            var jsonStringTest = File.ReadAllText(filePath);
            using var document = JsonDocument.Parse(jsonStringTest);
            var samples = document.RootElement.GetProperty("samples").Deserialize<PumpMeasurementSample[]>();
            
            var jsonStringExpect = File.ReadAllText(expectedPath);
            using var expectedDocument = JsonDocument.Parse(jsonStringExpect);
            var expected = expectedDocument.RootElement.GetProperty("results").GetProperty(fileName);

            // 1. Проецируем samples в measures
            var measures = samples.Select(s => new Measure(
                s.TimeSecond,
                s.Pressure,
                s.Rpm.HasValue ? (int)s.Rpm : null,
                s is {IsValid: true}
            ));

            // [Act]
            // 2. Расчет сессии
            var session = WorkSession.Create(measures.ToArray());
            session.CalculateAnalytics();

            // 3. Вычисление агрегированных величин по ТЗ
            var stoppedS = session.ModeDurations[WorkMode.Stopped];
            var gapS = session.ModeDurations[WorkMode.Gapped];
            var invalidS = session.ModeDurations[WorkMode.Unreached];
            var lowS = session.ModeDurations[WorkMode.Low];
            var normalS = session.ModeDurations[WorkMode.Normal];
            var highS = session.ModeDurations[WorkMode.High];

            var workingS = lowS + normalS + highS;
            var unknownS = gapS + invalidS;

            // [Assert]
            // 4. Проверка всех полей из expected_results.json
            Assert.Equal(expected.GetProperty("sample_count").Deserialize<double>(), session.Measures.Count);
            Assert.Equal(expected.GetProperty("span_s").Deserialize<double>(), session.Duration, 3);
            Assert.Equal(expected.GetProperty("working_s").Deserialize<double>(), workingS, 3);
            Assert.Equal(expected.GetProperty("stopped_s").Deserialize<double>(), stoppedS, 3);
            Assert.Equal(expected.GetProperty("gap_s").Deserialize<double>(), gapS, 3);
            Assert.Equal(expected.GetProperty("invalid_s").Deserialize<double>(), invalidS, 3);
            Assert.Equal(expected.GetProperty("unknown_s").Deserialize<double>(), unknownS, 3);

            // Проверка подрежимов
            var modes = expected.GetProperty("modes_s"); ;
            Assert.Equal(modes.GetProperty("low").Deserialize<double>(), lowS, 3);
            Assert.Equal(modes.GetProperty("normal").Deserialize<double>(), normalS, 3);
            Assert.Equal(modes.GetProperty("high").Deserialize<double>(), highS, 3);

            // 5. Контрольное уравнение из ТЗ: span_s = working_s + stopped_s + unknown_s
            Assert.Equal(session.Duration, workingS + stoppedS + unknownS, 3);
        }
    }
}