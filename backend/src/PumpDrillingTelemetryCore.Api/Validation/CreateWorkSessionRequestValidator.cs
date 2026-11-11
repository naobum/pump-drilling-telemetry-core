using FluentValidation;
using PumpDrillingTelemetryCore.Api.Requests.CreateWorkSession;

namespace PumpDrillingTelemetryCore.Api.Validation;

public sealed class CreateWorkSessionRequestValidator : AbstractValidator<CreateWorkSessionRequest>
{
    public CreateWorkSessionRequestValidator()
    {
        RuleFor(request => request.Samples).Custom((samples, context) =>
        {
            if (samples is null)
                return;

            var timestamps = new HashSet<double>();
            for (var index = 0; index < samples.Length; index++)
            {
                var sample = samples[index];
                if (!timestamps.Add(sample.TimeInSeconds))
                    context.AddFailure($"samples[{index}].t_s",
                        $"Duplicate t_s value {sample.TimeInSeconds} at samples[{index}]. Each sample must have a unique t_s.");
            }
        });
    }
}
