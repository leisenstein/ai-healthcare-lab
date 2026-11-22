using Anthropic;
using Microsoft.Extensions.AI;
using Triage.Api.Interfaces;
using Triage.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var apiKey = builder.Configuration["Anthropic:ApiKey"] ?? throw new InvalidOperationException("Set Anthropic:ApiKey in user-secrets");


IChatClient anthropicChatClient = new AnthropicClient { ApiKey = apiKey }.AsIChatClient("claude-sonnet-5-5");


builder.Services.AddChatClient(sp => new ChatClientBuilder(anthropicChatClient)
    .UseFunctionInvocation()
    .UseLogging()
    .Build(sp));

builder.Services.AddSingleton<ITriageResultParser, TriageResultParser>();
builder.Services.AddScoped<ITriageClassifier, TriageClassifier>();


var app = builder.Build();




app.MapGet("/", () => "Hello World!");
app.Run();
