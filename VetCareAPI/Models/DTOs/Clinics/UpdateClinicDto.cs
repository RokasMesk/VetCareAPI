namespace VetCareAPI.Models.DTOs.Clinics;
using System.ComponentModel.DataAnnotations;

public record class UpdateClinicDto
{
    [Required, StringLength(120)]
    public string Name { get; init; } = null!;

    [Required, StringLength(120)]
    public string City { get; init; } = null!;

    [Required, StringLength(50)]
    public string Phone { get; init; } = null!;

    [Required, StringLength(200)]
    public string Address { get; init; } = null!;

    [StringLength(500)]
    public string? Photo { get; init; }
}