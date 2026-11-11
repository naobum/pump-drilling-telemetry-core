using PumpDrillingTelemetryCore.Application.Commands.CreateWorkSession;

namespace PumpDrillingTelemetryCore.Application.Services;

public interface IWorkSessionStorage
{
    WorkSessionSummaryResponse? GetLatestSession();
    void SaveSession(WorkSessionSummaryResponse session);
}