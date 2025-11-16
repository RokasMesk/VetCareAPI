using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VetCareAPI.Models;

[Table("Users")]
[Index(nameof(Email), IsUnique = true)]
public class AppUser
{
    public Guid Id { get; set; }

    [MaxLength(120)]
    public string? FullName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;

    [MaxLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Role { get; set; } = Roles.User;
    

    // if its a simple user, and it has pets
    public ICollection<Pet>? Pets { get; set; } = new List<Pet>();
    
    // if user is a worker in a clinic
    public Guid? ClinicId { get; set; }
    public Clinic?  Clinic { get; set; }
}