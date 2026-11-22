## Triage eval (2026-11-22 10:06)

| Model | Category | Urgency | needsHuman | Missed escalations | Errors | Avg latency | Tokens in / out | Cost (run) | Cost per 1,000 msgs |
|---|---|---|---|---|---|---|---|---|---|
| claude-sonnet-5-5 | 100% | 93% | 87% | 0 | 0 | 2.9 s | 75,101 / 7,113 | $0.2213 | $7.38 |
| claude-haiku-5-5 | 93% | 97% | 90% | 0 | 0 | 3.0 s | 76,028 / 12,091 | $0.0136 | $0.45 |

### claude-sonnet-5-5: 6 of 30 not fully correct

| Id | Expected | Got | Note |
|---|---|---|---|
| m08 | refill / soon / human=false | refill / soon / human=true |  |
| m11 | scheduling / routine / human=false | scheduling / routine / human=true |  |
| m18 | billing / soon / human=false | billing / soon / human=true |  |
| m20 | clinical / soon / human=true | clinical / routine / human=true |  |
| m22 | billing / routine / human=false | billing / routine / human=true |  |
| m30 | clinical / routine / human=true | clinical / soon / human=true |  |

### claude-haiku-5-5: 6 of 30 not fully correct

| Id | Expected | Got | Note |
|---|---|---|---|
| m06 | refill / soon / human=false | refill / soon / human=true |  |
| m14 | billing / routine / human=false | billing / routine / human=true |  |
| m18 | billing / soon / human=false | billing / soon / human=true |  |
| m22 | billing / routine / human=false | scheduling / routine / human=false |  |
| m29 | refill / urgent / human=true | clinical / urgent / human=true |  |
| m30 | clinical / routine / human=true | clinical / soon / human=true |  |
