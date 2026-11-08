using MediatR;
using PumpDrillingTelemetryCore.Domain.Models;

namespace PumpDrillingTelemetryCore.Application.Commands.CreateWorkSession;

public record CreateWorkSessionCommand(Measure[] Measures)
    : IRequest<WorkSessionSummaryResponse>;