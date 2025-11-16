namespace VetCareAPI.Models.DTOs.Pets;

public record class PetDto(
    Guid Id,
    string Name,
    string Species,
    DateTime DateOfBirth,
    Guid UserId
);