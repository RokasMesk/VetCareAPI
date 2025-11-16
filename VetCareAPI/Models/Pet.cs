namespace VetCareAPI.Models;

public class Pet
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Species { get; set; } = null!;
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}