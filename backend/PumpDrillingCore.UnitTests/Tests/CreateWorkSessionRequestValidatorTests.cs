using PumpDrillingTelemetryCore.Api.Requests.CreateWorkSession;
using PumpDrillingTelemetryCore.Api.Validation;

namespace PumpDrillingCore.UnitTests.Tests;

public class CreateWorkSessionRequestValidatorTests
{
    private readonly CreateWorkSessionRequestValidator _validator = new();

    [Fact]
    public async Task Validate_DescendingTime_FailsValidation()
    {
        var request = new CreateWorkSessionRequest
        {
            SchemaVersion = "1.0",
            DatasetId = "test",
            Synthetic = true,
            Samples =
            [
                new MeasureDto { TimeInSeconds = 2.0, Pressure = 5, Rpm = 100, IsValid = true },
                new MeasureDto { TimeInSeconds = 1.0, Pressure = 5, Rpm = 100, IsValid = true }
            ]
        };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("strictly increasing"));
    }

    [Fact]
    public async Task Validate_NegativeTime_FailsValidation()
    {
        var request = new CreateWorkSessionRequest
        {
            SchemaVersion = "1.0",
            DatasetId = "test",
            Synthetic = true,
            Samples =
            [
                new MeasureDto { TimeInSeconds = -1.0, Pressure = 5, Rpm = 100, IsValid = true }
            ]
        };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("non-negative"));
    }
}