using AiHealthLab.Core.Llm;
using Anthropic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;
using Triage.Api.Data;
using Triage.Api.Endpoints;
using Triage.Api.Interfaces;
using Triage.Api.Services;
using Triage.Api.Tools;



var builder = WebApplication.CreateBuilder(args);
var apiKey = builder.Configuration["Anthropic:ApiKey"] ?? throw new InvalidOperationException("Set Anthropic:ApiKey in user-secrets");


// IChatClient anthropicChatClient = new AnthropicClient { ApiKey = apiKey }.AsIChatClient("claude-sonnet-5-5");
IChatClient anthropicChatClient = new AnthropicClient { ApiKey = apiKey }.AsIChatClient("claude-haiku-5-5");

var pricing = builder.Configuration.GetSection("LlmPricing").Get<LlmPricingOptions>() ?? new();
builder.Services.AddLlmCostTracking(pricing);

builder.Services.AddChatClient(sp => new ChatClientBuilder(anthropicChatClient)
    .UseFunctionInvocation()
    .UseLogging()
    .UseCostLogging()   // keep this last
    .Build(sp));

builder.Services.AddDbContext<AppDb>(o => o.UseSqlite(builder.Configuration.GetConnectionString("Triage")));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<INextAppointmentToolFactory, NextAppointmentToolFactory>();

builder.Services.AddSingleton<ITriageResultParser, TriageResultParser>();
builder.Services.AddScoped<ITriageClassifier, TriageClassifier>();
builder.Services.AddScoped<ITriageService, TriageService>();
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDb>().Database.EnsureCreated();


app.MapTriageEndpoints();
app.Run();


public partial class Program;