namespace VetCareAPI.Models.DTOs.Pets;
using System;
using System.ComponentModel.DataAnnotations;

public record class CreatePetDto
{
    [Required, StringLength(80)]
    public string Name { get; init; } = null!;

    [Required, StringLength(40)]
    public string Species { get; init; } = null!; 
    public DateTime DateOfBirth { get; init; }

    [Required]
    public Guid UserId { get; set; }
}