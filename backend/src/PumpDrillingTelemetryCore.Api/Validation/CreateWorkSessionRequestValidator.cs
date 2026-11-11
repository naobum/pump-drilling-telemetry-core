using FluentValidation;
using PumpDrillingTelemetryCore.Api.Requests.CreateWorkSession;

namespace PumpDrillingTelemetryCore.Api.Validation;

public sealed class CreateWorkSessionRequestValidator : AbstractValidator<CreateWorkSessionRequest>
{
    public CreateWorkSessionRequestValidator()
    {
        RuleFor(request => request.Samples).Custom((samples, context) =>
        {
            if (samples is null || samples.Length == 0)
                return;

            double? prevTime = null;
            for (var index = 0; index < samples.Length; index++)
            {
                var sample = samples[index];

                if (double.IsNaN(sample.TimeInSeconds) || double.IsInfinity(sample.TimeInSeconds) || sample.TimeInSeconds < 0)
                {
                    context.AddFailure($"samples[{index}].t_s",
                        $"Invalid timestamp {sample.TimeInSeconds} at samples[{index}]. Value must be a non-negative finite number.");
                    return;
                }

                if (prevTime.HasValue)
                {
                    if (sample.TimeInSeconds == prevTime.Value)
                    {
                        context.AddFailure($"samples[{index}].t_s",
                            $"Duplicate t_s value {sample.TimeInSeconds} at samples[{index}]. Each sample must have a unique t_s.");
                        return;
                    }

                    if (sample.TimeInSeconds < prevTime.Value)
                    {
                        context.AddFailure($"samples[{index}].t_s",
                            $"Non-increasing t_s value {sample.TimeInSeconds} at samples[{index}]. Timestamps must be strictly increasing.");
                        return;
                    }
                }

                prevTime = sample.TimeInSeconds;
            }
        });
    }
}