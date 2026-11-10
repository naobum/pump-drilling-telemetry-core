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
        var summary = workSession.GetAnalytics();
        var response = summary.ToResponse();

        return Task.FromResult(response);
    }
}