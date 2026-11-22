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


## Agent skills

### Issue tracker

Issues live in GitHub Issues for leisenstein/ai-healthcare-lab (via the `gh` CLI). See `docs/agents/issue-tracker.md`.

### Triage labels

Default five-role vocabulary: `needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: one `CONTEXT.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.
