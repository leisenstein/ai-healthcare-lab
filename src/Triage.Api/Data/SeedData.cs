namespace Triage.Api.Data;

/// <summary>
/// 10 synthetic patients (P001-P010, matching data/messages.json) and their appointments.
/// Every name and visit is invented. P007 has no future appointment on purpose, and
/// P003 has a past visit so "next" has to skip it.
/// </summary>
public static class SeedData
{
    public static readonly Patient[] Patients =
    [
        new() { Id = "P001", DisplayName = "Avery Testwell" },
        new() { Id = "P002", DisplayName = "Jordan Sampleton" },
        new() { Id = "P003", DisplayName = "Casey Mockford" },
        new() { Id = "P004", DisplayName = "Riley Fakename" },
        new() { Id = "P005", DisplayName = "Morgan Placeholder" },
        new() { Id = "P006", DisplayName = "Quinn Synthetic" },
        new() { Id = "P007", DisplayName = "Taylor Demo" },
        new() { Id = "P008", DisplayName = "Drew Example" },
        new() { Id = "P009", DisplayName = "Skyler Dummy" },
        new() { Id = "P010", DisplayName = "Reese Notreal" },
    ];

    public static readonly Appointment[] Appointments =
    [
        new() { Id = 1,  PatientId = "P001", StartsAt = new(2026, 12, 3, 9, 30, 0),   Provider = "Dr. Lane",  Reason = "Follow-up" },
        new() { Id = 2,  PatientId = "P001", StartsAt = new(2027, 1, 14, 8, 0, 0),    Provider = "Dr. Lane",  Reason = "Annual physical" },
        new() { Id = 3,  PatientId = "P002", StartsAt = new(2026, 12, 8, 14, 0, 0),   Provider = "Dr. Patel", Reason = "Blood pressure check" },
        new() { Id = 4,  PatientId = "P003", StartsAt = new(2026, 10, 20, 10, 0, 0),  Provider = "Dr. Kim",   Reason = "Cholesterol review" },
        new() { Id = 5,  PatientId = "P003", StartsAt = new(2027, 1, 5, 10, 0, 0),    Provider = "Dr. Kim",   Reason = "Cholesterol review" },
        new() { Id = 6,  PatientId = "P004", StartsAt = new(2026, 12, 1, 11, 15, 0),  Provider = "Dr. Lane",  Reason = "Lab results" },
        new() { Id = 7,  PatientId = "P005", StartsAt = new(2026, 12, 10, 15, 30, 0), Provider = "Dr. Ortiz", Reason = "Thyroid follow-up" },
        new() { Id = 8,  PatientId = "P006", StartsAt = new(2026, 11, 23, 14, 0, 0),  Provider = "Dr. Patel", Reason = "Diabetes check" },
        new() { Id = 9,  PatientId = "P008", StartsAt = new(2026, 11, 30, 9, 0, 0),   Provider = "Dr. Kim",   Reason = "Asthma follow-up" },
        new() { Id = 10, PatientId = "P009", StartsAt = new(2026, 12, 2, 16, 0, 0),   Provider = "Dr. Ortiz", Reason = "INR check" },
        new() { Id = 11, PatientId = "P010", StartsAt = new(2026, 12, 15, 13, 0, 0),  Provider = "Dr. Lane",  Reason = "Dermatology referral" },
    ];
}
