using System.Text.Json;
using PumpDrillingTelemetryCore.Application.Commands.CreateWorkSession;
using PumpDrillingTelemetryCore.Domain.Models;
using Xunit;

namespace PumpDrillingCore.UnitTests.tests;

public class CreateWorkSessionCommandHandlerTests
{
    [Fact]
    public async Task Handle_MixedModes_ReturnsExpectedDurations()
    {
        Measure[] measures =
        [
            new(0, 2, 100, true),      // low: 1 секунда
            new(1, 6, 100, true),      // normal: 1,5 секунды
            new(2.5, 12, 100, true),   // high: 0,5 секунды
            new(3, 6, 0, true),        // остановка: 1 секунда
            new(4, null, 100, false),  // недостоверно: 1 секунда
            new(5, null, null, false), // пропуск: 3 секунды
            new(8, 6, 100, true)       // последняя точка
        ];

        var handler = new CreateWorkSessionCommandHandler();

        var result = await handler.Handle(
            new CreateWorkSessionCommand(measures),
            CancellationToken.None);

        Assert.Equal(3.0, result.WorkingTime);
        Assert.Equal(1.0, result.StoppedTime);
        Assert.Equal(3.0, result.GappedTime);
        Assert.Equal(1.0, result.InvalidTime);
        Assert.Equal(4.0, result.UnknownTime);
        Assert.Equal(8.0, result.SpanTime);

        Assert.Equal(3, result.ModesWorkTime.Count);
        Assert.Equal(1.0, result.ModesWorkTime["low"]);
        Assert.Equal(1.5, result.ModesWorkTime["normal"]);
        Assert.Equal(0.5, result.ModesWorkTime["high"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Handle_EmptyOrSinglePoint_ReturnsZeroDurations(
        int measureCount)
    {
        Measure[] measures = measureCount == 0
            ? []
            : [new Measure(10, 6, 100, true)];

        var handler = new CreateWorkSessionCommandHandler();

        var result = await handler.Handle(
            new CreateWorkSessionCommand(measures),
            CancellationToken.None);

        Assert.Equal(0.0, result.WorkingTime);
        Assert.Equal(0.0, result.StoppedTime);
        Assert.Equal(0.0, result.GappedTime);
        Assert.Equal(0.0, result.InvalidTime);
        Assert.Equal(0.0, result.UnknownTime);
        Assert.Equal(0.0, result.SpanTime);

        Assert.Equal(3, result.ModesWorkTime.Count);
        Assert.Equal(0.0, result.ModesWorkTime["low"]);
        Assert.Equal(0.0, result.ModesWorkTime["normal"]);
        Assert.Equal(0.0, result.ModesWorkTime["high"]);
    }

    [Fact]
    public void Response_Serialization_UsesRequiredJsonNames()
    {
        var response = new WorkSessionSummaryResponse(
            WorkingTime: 6,
            StoppedTime: 4,
            GappedTime: 5,
            InvalidTime: 7,
            UnknownTime: 12,
            SpanTime: 22,
            ModesWorkTime: new Dictionary<string, double>
            {
                ["low"] = 1,
                ["normal"] = 2,
                ["high"] = 3
            });

        var json = JsonSerializer.Serialize(response);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal(7, root.EnumerateObject().Count());
        Assert.Equal(6.0, root.GetProperty("working_s").GetDouble());
        Assert.Equal(4.0, root.GetProperty("stopped_s").GetDouble());
        Assert.Equal(5.0, root.GetProperty("gap_s").GetDouble());
        Assert.Equal(7.0, root.GetProperty("invalid_s").GetDouble());
        Assert.Equal(12.0, root.GetProperty("unknown_s").GetDouble());
        Assert.Equal(22.0, root.GetProperty("span_s").GetDouble());

        var modes = root.GetProperty("modes_s");

        Assert.Equal(3, modes.EnumerateObject().Count());
        Assert.Equal(1.0, modes.GetProperty("low").GetDouble());
        Assert.Equal(2.0, modes.GetProperty("normal").GetDouble());
        Assert.Equal(3.0, modes.GetProperty("high").GetDouble());
    }
}