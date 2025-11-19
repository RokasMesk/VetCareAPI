namespace VetCareAPI.Models.DTOs.Pets;

using System;
using System.ComponentModel.DataAnnotations;

public record class UpdatePetDto
{
    [Required, StringLength(80)]
    public string Name { get; init; } = null!;

    [Required, StringLength(40)]
    public string Species { get; init; } = null!;

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; init; }
}
