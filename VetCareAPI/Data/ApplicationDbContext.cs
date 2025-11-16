// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VetCareAPI.Models;

namespace VetCareAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);

        // ----- Relationships -----
        model.Entity<Pet>()
            .HasOne(p => p.User)
            .WithMany(u => u.Pets)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        model.Entity<Visit>()
            .HasOne(v => v.Clinic)
            .WithMany(c => c.Visits)
            .HasForeignKey(v => v.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        model.Entity<Visit>()
            .HasOne(v => v.Pet)
            .WithMany(p => p.Visits)
            .HasForeignKey(v => v.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        // ----- AppUser -----
        model.Entity<AppUser>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(255).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(200).IsRequired();
            e.Property(u => u.Role).HasMaxLength(32).IsRequired();

            // Optional DB CHECK for roles (MySQL 8+)
            e.ToTable(t => t.HasCheckConstraint("CK_Users_Role",
                $"Role IN ('{Roles.Admin}','{Roles.ClinicStaff}','{Roles.User}')"));

            // Clinic relation (one clinic -> many users)
            e.HasOne(u => u.Clinic)
             .WithMany(c => c.Staff) // ensure Clinic has ICollection<AppUser> Staff { get; set; } = new();
             .HasForeignKey(u => u.ClinicId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ----- RefreshToken -----
        model.Entity<RefreshToken>(e =>
        {
            e.HasIndex(rt => rt.TokenHash).IsUnique();
            e.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(256);
            e.HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ----- Store Visit enums as strings -----
        model.Entity<Visit>().Property(v => v.Status).HasConversion<string>().HasMaxLength(32);
        model.Entity<Visit>().Property(v => v.Reason).HasConversion<string>().HasMaxLength(32);
        model.Entity<Visit>().Property(v => v.Severity).HasConversion<string>().HasMaxLength(32);

        // ----- Force UTC -----
        var utc = new ValueConverter<DateTime, DateTime>(
            toProvider => toProvider.Kind == DateTimeKind.Utc ? toProvider : toProvider.ToUniversalTime(),
            fromProvider => DateTime.SpecifyKind(fromProvider, DateTimeKind.Utc)
        );
        model.Entity<Visit>().Property(v => v.StartsAt).HasColumnType("datetime(6)").HasConversion(utc);
        model.Entity<Visit>().Property(v => v.EndsAt).HasColumnType("datetime(6)").HasConversion(utc);

        // ----- Seed data -----

        // Clinics
        var clinicKaunas = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var clinicVilnius = Guid.Parse("11111111-1111-1111-1111-222222222222");

        model.Entity<Clinic>().HasData(
            new Clinic { Id = clinicKaunas, Name = "ZooVet Kaunas", City = "Kaunas", Address = "Laisvės al. 1" },
            new Clinic { Id = clinicVilnius, Name = "VetHelp Vilnius", City = "Vilnius", Address = "Gedimino pr. 2" }
        );

        // Users (using your precomputed hashes; no UserName used)
        var adminId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var ownerId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var staffId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        model.Entity<AppUser>().HasData(
            new AppUser
            {
                Id = adminId,
                FullName = "System Admin",
                Email = "admin@gmail.com",
                PasswordHash = "AQAAAAIAAYagAAAAEFBlcphBR2VdzREjobOqd+lyX079u33WlqzjAnURuM/kwQlKCpy2wZ9nsLSK4blkvg==",
                Role = Roles.Admin,
                ClinicId = null
            },
            new AppUser
            {
                Id = ownerId,
                FullName = "Demo Owner",
                Email = "owner@gmail.com",
                PasswordHash = "AQAAAAIAAYagAAAAEL2Z/MDnmseAtqaj5Rvr/0HSykjkIuh/D5b5aHUdn1yiqNI9BUHoCxlUZYhA2eInBw==",
                Role = Roles.User,
                ClinicId = null
            },
            new AppUser
            {
                Id = staffId,
                FullName = "Kaunas Vet Staff",
                Email = "staff@gmail.com",
                PasswordHash = "AQAAAAIAAYagAAAAEP3ggSAiRexQx1Oe6+uWxRHVlvNEsJ1YMv+Ma9M3WL5HujKS3pVHg1OBJ9GKMGrtyA==",
                Role = Roles.ClinicStaff,
                ClinicId = clinicKaunas
            }
        );

        // Pets (owned by the Owner user)
        var petMaksis = Guid.Parse("33333333-3333-3333-3333-111111111111");
        var petMurka = Guid.Parse("33333333-3333-3333-3333-222222222222");
        var petPukis = Guid.Parse("33333333-3333-3333-3333-333333333333");

        model.Entity<Pet>().HasData(
            new Pet { Id = petMaksis, Name = "Maksis", Species = "Dog", UserId = ownerId },
            new Pet { Id = petMurka, Name = "Murka", Species = "Cat", UserId = ownerId },
            new Pet { Id = petPukis, Name = "Pūkis", Species = "Rabbit", UserId = ownerId }
        );

        // Visits
        var v1Completed = Guid.Parse("44444444-4444-4444-4444-111111111111");
        var v2Scheduled = Guid.Parse("44444444-4444-4444-4444-222222222222");
        var v3Scheduled = Guid.Parse("44444444-4444-4444-4444-333333333333");
        var v4Cancelled = Guid.Parse("44444444-4444-4444-4444-444444444444");

        model.Entity<Visit>().HasData(
            new Visit
            {
                Id = v1Completed,
                StartsAt = new DateTime(2025, 09, 15, 09, 00, 00, DateTimeKind.Utc),
                EndsAt = new DateTime(2025, 09, 15, 09, 30, 00, DateTimeKind.Utc),
                Notes = "Vaccination completed.",
                Status = VisitStatus.Completed,
                Reason = VisitReason.Vaccination,
                ChiefComplaint = null,
                DiagnosisCode = null,
                DiagnosisText = null,
                Severity = Severity.Mild,
                ClinicId = clinicKaunas,
                PetId = petMaksis
            },
            new Visit
            {
                Id = v2Scheduled,
                StartsAt = new DateTime(2025, 10, 12, 08, 30, 00, DateTimeKind.Utc),
                EndsAt = new DateTime(2025, 10, 12, 09, 00, 00, DateTimeKind.Utc),
                Notes = "Annual check.",
                Status = VisitStatus.Scheduled,
                Reason = VisitReason.Checkup,
                ChiefComplaint = "Limping on right hind leg",
                DiagnosisCode = null,
                DiagnosisText = null,
                Severity = Severity.Moderate,
                ClinicId = clinicKaunas,
                PetId = petMaksis
            },
            new Visit
            {
                Id = v3Scheduled,
                StartsAt = new DateTime(2025, 10, 20, 14, 00, 00, DateTimeKind.Utc),
                EndsAt = new DateTime(2025, 10, 20, 14, 30, 00, DateTimeKind.Utc),
                Notes = "Dental check.",
                Status = VisitStatus.Scheduled,
                Reason = VisitReason.Dental,
                ChiefComplaint = "Bad breath",
                DiagnosisCode = null,
                DiagnosisText = null,
                Severity = Severity.Mild,
                ClinicId = clinicVilnius,
                PetId = petMurka
            },
            new Visit
            {
                Id = v4Cancelled,
                StartsAt = new DateTime(2025, 10, 05, 16, 00, 00, DateTimeKind.Utc),
                EndsAt = new DateTime(2025, 10, 05, 16, 30, 00, DateTimeKind.Utc),
                Notes = "Owner cancelled day before.",
                Status = VisitStatus.Cancelled,
                Reason = VisitReason.Illness,
                ChiefComplaint = "Loss of appetite",
                DiagnosisCode = null,
                DiagnosisText = null,
                Severity = Severity.Moderate,
                ClinicId = clinicVilnius,
                PetId = petPukis
            }
        );
    }
}
