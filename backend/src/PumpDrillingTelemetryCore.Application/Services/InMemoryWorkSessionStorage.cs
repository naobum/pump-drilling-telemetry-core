using PumpDrillingTelemetryCore.Application.Commands.CreateWorkSession;

namespace PumpDrillingTelemetryCore.Application.Services;

public sealed class InMemoryWorkSessionStorage : IWorkSessionStorage
{
    private WorkSessionSummaryResponse? _latestSession;
    private readonly object _lock = new();

    public WorkSessionSummaryResponse? GetLatestSession()
    {
        lock (_lock)
        {
            return _latestSession;
        }
    }

    public void SaveSession(WorkSessionSummaryResponse session)
    {
        lock (_lock)
        {
            _latestSession = session;
        }
    }
}