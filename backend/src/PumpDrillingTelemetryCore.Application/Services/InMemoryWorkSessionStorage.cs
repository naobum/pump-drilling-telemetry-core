using PumpDrillingTelemetryCore.Application.Commands.CreateWorkSession;

namespace PumpDrillingTelemetryCore.Application.Services;

public sealed class InMemoryWorkSessionStorage : IWorkSessionStorage
{
    private WorkSessionDetailsResponse? _latestSession;
    private readonly object _lock = new();

    public WorkSessionDetailsResponse? GetLatestSession()
    {
        lock (_lock)
        {
            return _latestSession;
        }
    }

    public void SaveSession(WorkSessionDetailsResponse session)
    {
        lock (_lock)
        {
            _latestSession = session;
        }
    }
}