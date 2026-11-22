# AI Healthcare Lab

.NET 10 / C# projects for AI integration in healthcare, built while learning. **Synthetic data only.**

## Week 1: Patient message triage API (`src/Triage.Api`)

`POST /triage` takes `{ patientId, message }` from a patient portal and returns JSON a front desk can act on:

```json
{ "patientId": "P001", "category": "scheduling", "urgency": "routine", "needsHuman": false,
  "summary": "Asks to move Thursday's visit", "draftReply": "...", "nextAppointment": "2026-12-03T09:30:00" }
```

How it works:

- The model (Claude, via `Microsoft.Extensions.AI` `IChatClient`) classifies the message and drafts a reply.
- It can call one tool, `get_next_appointment`, which reads SQLite. The tool is locked to the request's patient.
- `patientId` and `nextAppointment` come from the request and the database, never from the model.
- The JSON is validated. If it's invalid, the call is retried once with the error.
- Clinical or urgent messages always get `needsHuman: true`. This is enforced in C# (`SafetyRules`), not only in the prompt.
- Every model call logs model, tokens, latency and cost. `GET /stats` shows the totals.

Run: `dotnet run --project src/Triage.Api` (needs `Anthropic:ApiKey` in user-secrets).
Tests: `dotnet test --filter Category!=Eval` (free) · Eval: `dotnet test --filter Category=Eval` (paid, see below).

## Results (Week 1 eval, 30 synthetic messages)

| Model | Category | Urgency | needsHuman | Missed escalations | Errors | Avg latency | Tokens in / out | Cost (run) | Cost per 1,000 msgs |
|---|---|---|---|---|---|---|---|---|---|
| claude-sonnet-5-5 | 100% | 93% | 87% | 0 | 0 | 2.9 s | 75,101 / 7,113 | $0.2213 | $7.38 |
| claude-haiku-5-5 | 93% | 97% | 90% | 0 | 0 | 3.0 s | 76,028 / 12,091 | $0.0136 | $0.45 |

**Recommendation: ship Haiku.** It was as accurate as Sonnet overall. It was better on urgency and on `needsHuman`, slightly worse on category, and just as fast, at about 1/16 of the cost ($0.45 vs $7.38 per 1,000 messages). Neither model missed a message that needed a human, which is the number that matters most. The C# safety rule backs that up no matter which model runs. Use Sonnet only if category accuracy turns out to matter more on real traffic.

**What the misses tell us:** most misses were the model sending a message to a human when the label said it didn't need one. That's the safe direction, and it costs staff time, not patient safety. Haiku's one category miss (a missed blood thinner labeled "refill", called "clinical") was still marked urgent and sent to a human. Caveat: 30 messages is a small sample. Re-run the eval on a larger, messier set before relying on it.

Full per-message details are in `data/eval-results.md`. Re-run with `EVAL_MODELS` (`sonnet`, `haiku`, `opus`) and `EVAL_LIMIT` (first N messages) to control cost.
