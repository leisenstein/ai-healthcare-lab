using Microsoft.EntityFrameworkCore;

namespace Triage.Api.Data;

/// <summary>EF Core SQLite database for synthetic patients and appointments.</summary>
public sealed class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasMany(p => p.Appointments).WithOne().HasForeignKey(a => a.PatientId);
            e.HasData(SeedData.Patients.Select(p => new { p.Id, p.DisplayName }));
        });

        modelBuilder.Entity<Appointment>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => new { a.PatientId, a.StartsAt });
            e.HasData(SeedData.Appointments);
        });
    }
}
