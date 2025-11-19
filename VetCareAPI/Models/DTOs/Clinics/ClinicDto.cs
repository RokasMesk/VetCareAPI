namespace VetCareAPI.Models.DTOs.Clinics;

public record class ClinicDto(
    Guid Id,
    string Name,
    string City,
    string Phone,
    string Address,
    string? Photo
);

