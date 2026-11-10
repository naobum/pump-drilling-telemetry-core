using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PumpDrillingCore.FunctionalTests.Tests;

// Run the API with the http launch profile before running these integration tests.
public sealed class WorkSessionImportTests : IDisposable
{
    private readonly HttpClient _client = new()
    {
        BaseAddress = new Uri(Environment.GetEnvironmentVariable("PUMP_API_URL") ?? "http://localhost:5112")
    };

    public static IEnumerable<object[]> Files => Directory.GetFiles(
        Path.Combine(AppContext.BaseDirectory, "TestData"), "*.json")
        .Where(path => Path.GetFileName(path) != "expected_results.json")
        .OrderBy(path => path)
        .Select(path => new object[] { Path.GetFileName(path) });

    [Theory]
    [MemberData(nameof(Files))]
    public async Task Import_MatchesExpectedResult(string fileName)
    {
        using var expectedDocument = JsonDocument.Parse(
            await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "TestData", "expected_results.json"),
                TestContext.Current.CancellationToken));
        var expected = expectedDocument.RootElement.GetProperty("results").GetProperty(fileName);
        using var form = new MultipartFormDataContent();
        using var file = new StreamContent(File.OpenRead(Path.Combine(AppContext.BaseDirectory, "TestData", fileName)));
        file.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        form.Add(file, "file", fileName);

        using var response = await _client.PostAsync("/api/pump/work-session/import", form, TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var success = expected.GetProperty("status").GetString() == "ok";
        Assert.True(response.StatusCode == (success ? HttpStatusCode.OK : HttpStatusCode.BadRequest),
            $"{fileName}: HTTP {(int)response.StatusCode}: {body}");
        using var actual = JsonDocument.Parse(body);
        if (!success)
        {
            Assert.False(string.IsNullOrWhiteSpace(body));
            Assert.False(actual.RootElement.TryGetProperty("working_s", out _));
            return;
        }

        foreach (var field in new[] { "span_s", "working_s", "stopped_s", "gap_s", "invalid_s", "unknown_s" })
            Assert.Equal(expected.GetProperty(field).GetDouble(), actual.RootElement.GetProperty(field).GetDouble(), 3);
        foreach (var mode in new[] { "low", "normal", "high" })
            Assert.Equal(expected.GetProperty("modes_s").GetProperty(mode).GetDouble(),
                actual.RootElement.GetProperty("modes_s").GetProperty(mode).GetDouble(), 3);
    }

    public void Dispose() => _client.Dispose();
}
