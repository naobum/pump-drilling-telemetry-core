using PumpDrillingTelemetryCore.Application.Commands.CreateWorkSession;

namespace PumpDrillingTelemetryCore.Application.Services;

public interface IWorkSessionStorage
{
    WorkSessionDetailsResponse? GetLatestSession();
    void SaveSession(WorkSessionDetailsResponse session);
}