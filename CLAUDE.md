# AI Healthcare Lab
.NET 10 / C# solution. AI integration projects for healthcare.

## Commands
- Build: dotnet build
- Unit tests: dotnet test --filter Category!=Eval
- Evals (cost money): dotnet test --filter Category=Eval
- Local services: docker compose up -d

## Rules
- NEVER use real patient data. Synthetic data only.
- Secrets via dotnet user-secrets or env vars. Never commit keys or passwords.
- Model calls go through Microsoft.Extensions.AI IChatClient.
- Every model call logs model, token in/out, latency, cost.
- New services get xUnit tests. Run unit tests before saying done.
- Minimal API endpoints stay thin; logic lives in DI services.

## Patient triage rules (Triage.Api)
- Categories: refill, scheduling, billing, clinical. Urgency: routine, soon, urgent. Don't add or rename values without updating the eval labels.
- Clinical or urgent ALWAYS means needsHuman = true. This is enforced in C# (`SafetyRules`) after every model call, not just in the prompt. Never weaken, bypass or remove it. Rules may only make results more cautious.
- The model never sets patientId or nextAppointment. They come from the request and the database.
- get_next_appointment is locked to the request's patient. Never let a model-supplied ID read another patient's data.
- Never log patient message text. Logs hold IDs, categories, token counts and cost only.
- After changing the prompt or the model, re-run the eval. Missed escalations must stay at 0.

## Agent skills

### Issue tracker

Issues live in GitHub Issues for leisenstein/ai-healthcare-lab (via the `gh` CLI). See `docs/agents/issue-tracker.md`.

### Triage labels

Default five-role vocabulary: `needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: one `CONTEXT.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.
