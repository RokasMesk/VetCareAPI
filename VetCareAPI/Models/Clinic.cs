namespace VetCareAPI.Models;

public class Clinic
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string City { get; set; }
    public string Address { get; set; }
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    public ICollection<AppUser> Staff { get; set; } = new List<AppUser>();
}