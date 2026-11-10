using MediatR;
using PumpDrillingTelemetryCore.Domain.Models;

namespace PumpDrillingTelemetryCore.Application.Commands.CreateWorkSession;

public class CreateWorkSessionCommandHandler
    : IRequestHandler<CreateWorkSessionCommand, WorkSessionSummaryResponse>
{
    public Task<WorkSessionSummaryResponse> Handle(
        CreateWorkSessionCommand request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request.Measures);

        var workSession = WorkSession.Create(request.Measures);
        workSession.CalculateAnalytics();
        var response = workSession.ToResponse();

        return Task.FromResult(response);
    }
}