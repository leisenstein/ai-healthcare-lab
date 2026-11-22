using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AiHealthLab.Core.Llm;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Triage.Api.Interfaces;
using Triage.Api.Services;
using Triage.Tests.Fakes;

namespace Triage.Tests.Endpoints;

/// <summary>
/// The contract for POST /triage and GET /stats. These are written FIRST: they fail until
/// the endpoints exist, and pass once they behave as described. The real model is never
/// called: ITriageService is replaced by a fake.
/// </summary>
public class TriageEndpointsTests : IDisposable
{
    private readonly FakeTriageService _triage = new();
    private readonly InMemoryUsageStore _usage = new();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TriageEndpointsTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(web =>
        {
            web.UseSetting("ConnectionStrings:Triage", "Data Source=file:endpoint-tests?mode=memory&cache=shared");
            web.ConfigureTestServices(services =>
            {
                services.RemoveAll<ITriageService>();
                services.AddSingleton<ITriageService>(_triage);
                services.RemoveAll<IUsageStore>();
                services.AddSingleton<IUsageStore>(_usage);
            });
        });
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Post_triage_returns_200_with_camelCase_json_and_lowercase_enums()
    {
        var response = await _client.PostAsJsonAsync("/triage", new { patientId = "P004", message = "chest pain" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"patientId\":\"P004\"", json);
        Assert.Contains("\"category\":\"clinical\"", json);
        Assert.Contains("\"urgency\":\"urgent\"", json);
        Assert.Contains("\"needsHuman\":true", json);
        Assert.Contains("\"nextAppointment\":\"2026-12-01T11:15:00\"", json);

        var sent = Assert.Single(_triage.Requests);
        Assert.Equal("P004", sent.PatientId);
        Assert.Equal("chest pain", sent.Message);
    }

    [Theory]
    [InlineData("", "hello")]
    [InlineData("P001", "")]
    [InlineData("   ", "hello")]
    [InlineData("P001", "   ")]
    public async Task Post_triage_returns_400_for_blank_fields_and_never_calls_the_model(string patientId, string message)
    {
        var response = await _client.PostAsJsonAsync("/triage", new { patientId, message });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_triage.Requests);
    }

    [Fact]
    public async Task Post_triage_returns_400_for_a_message_over_4000_characters()
    {
        var response = await _client.PostAsJsonAsync("/triage", new { patientId = "P001", message = new string('a', 4001) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(_triage.Requests);
    }

    [Fact]
    public async Task Post_triage_returns_502_when_the_model_never_produces_valid_json()
    {
        _triage.Throw = new TriageParseException("No JSON object was found in the reply.");

        var response = await _client.PostAsJsonAsync("/triage", new { patientId = "P001", message = "my refill" });

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("my refill", body);   // never echo patient text in errors
    }

    [Fact]
    public async Task Get_stats_returns_the_usage_summary()
    {
        _usage.Add(new ModelCallRecord(DateTimeOffset.UtcNow, "claude-sonnet-5-5", 1000, 500, TimeSpan.FromMilliseconds(900), 0.007m));
        _usage.Add(new ModelCallRecord(DateTimeOffset.UtcNow, "claude-haiku-5-5", 1000, 500, TimeSpan.FromMilliseconds(300), 0.00035m));

        var response = await _client.GetAsync("/stats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        Assert.Equal(2, root.GetProperty("calls").GetInt32());
        Assert.Equal(2000, root.GetProperty("inputTokens").GetInt64());
        Assert.Equal(0.00735m, root.GetProperty("costUsd").GetDecimal());
        Assert.Equal(2, root.GetProperty("byModel").GetArrayLength());
    }

    [Fact]
    public async Task Get_stats_with_no_calls_returns_zeros()
    {
        var response = await _client.GetAsync("/stats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(0, doc.RootElement.GetProperty("calls").GetInt32());
    }
}
