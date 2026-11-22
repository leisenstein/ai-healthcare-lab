namespace Triage.Tests.Fakes;

/// <summary>A clock frozen at one moment, in UTC, so date tests never drift.</summary>
public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
