namespace VetCareAPI.Models.DTOs.Users;

public record class AppUserDto(
    Guid Id,
    string FullName,
    string Email,
    string Password
);