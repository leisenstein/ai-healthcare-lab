using Triage.Api.Contracts;

namespace Triage.Tests.Eval;

/// <summary>The answer you expected for one message (the "label").</summary>
public sealed record ExpectedLabel(TriageCategory Category, Urgency Urgency, bool NeedsHuman);

/// <summary>One labeled message from data/messages.json.</summary>
public sealed record EvalCase(string Id, string PatientId, string Message, ExpectedLabel Expected, string? Notes = null);
