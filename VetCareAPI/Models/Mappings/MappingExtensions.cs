
using VetCareAPI.Models.DTOs.Clinics;
using VetCareAPI.Models.DTOs.Pets;
using VetCareAPI.Models.DTOs.Users;
using VetCareAPI.Models.DTOs.Visits;

namespace VetCareAPI.Models.Mappings;

public static class MappingExtensions
{
    public static ClinicDto ToDto(this Clinic e) =>
        new(e.Id, e.Name, e.City, e.Address);

    public static Clinic ToEntity(this CreateClinicDto d) =>
        new() { Id = Guid.NewGuid(), Name = d.Name, City = d.City, Address = d.Address };

    public static void Apply(this Clinic e, UpdateClinicDto d)
    { e.Name = d.Name; e.City = d.City; e.Address = d.Address; }

    public static AppUserDto ToDto(this AppUser u) =>
        new(u.Id, u.FullName, u.Email, u.PasswordHash);

    public static AppUser ToEntity(this CreateAppUserDto d) =>
        new() { Id = Guid.NewGuid(), FullName = d.FullName, Email = d.Email };

    public static PetDto ToDto(this Pet p) =>
        new(p.Id, p.Name, p.Species, p.DateOfBirth, p.UserId);

    public static Pet ToEntity(this CreatePetDto d) =>
        new() { Id = Guid.NewGuid(), Name = d.Name, Species = d.Species, UserId = d.UserId };

    public static void Apply(this Pet p, UpdatePetDto d)
    { p.Name = d.Name; p.Species = d.Species; }

    public static VisitDto ToDto(this Visit v) =>
        new(v.Id, v.StartsAt, v.EndsAt, v.Notes, v.Status, v.Reason,
            v.ChiefComplaint, v.DiagnosisCode, v.DiagnosisText, v.Severity,
            v.ClinicId, v.PetId);

    public static Visit ToEntity(this CreateVisitDto d)
    {
        var v = new Visit {
            Id = Guid.NewGuid(),
            StartsAt = DateTime.SpecifyKind(d.StartsAt, DateTimeKind.Utc),
            EndsAt   = DateTime.SpecifyKind(d.EndsAt,   DateTimeKind.Utc),
            Notes = d.Notes,
            ClinicId = d.ClinicId,
            PetId = d.PetId,
            Status = VisitStatus.Scheduled,
            Reason = d.Reason.HasValue ? (VisitReason)d.Reason.Value : VisitReason.Checkup,
            ChiefComplaint = d.ChiefComplaint,
            DiagnosisCode  = d.DiagnosisCode,
            DiagnosisText  = d.DiagnosisText,
            Severity = d.Severity.HasValue ? (Severity?)d.Severity.Value : null
        };
        return v;
    }

    public static void Apply(this Visit v, UpdateVisitDto d)
    {
        v.StartsAt = DateTime.SpecifyKind(d.StartsAt, DateTimeKind.Utc);
        v.EndsAt   = DateTime.SpecifyKind(d.EndsAt,   DateTimeKind.Utc);
        v.Notes    = d.Notes;
        v.Status   = d.Status;
        v.Reason   = d.Reason;
        v.ChiefComplaint = d.ChiefComplaint;
        v.DiagnosisCode  = d.DiagnosisCode;
        v.DiagnosisText  = d.DiagnosisText;
        v.Severity       = d.Severity;
    }
}
