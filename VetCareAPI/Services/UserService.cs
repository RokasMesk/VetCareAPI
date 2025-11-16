using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetCareAPI.Data;
using VetCareAPI.Models;

namespace VetCareAPI.Services;

public interface IUserService
{
    Task<AppUser?> FindByEmailAsync(string email, CancellationToken ct);
    Task<AppUser> CreateAsync(string email, string password, string role, string? fullName, Guid? clinicId, CancellationToken ct);
    bool VerifyPassword(AppUser user, string password);
}

public class UserService(ApplicationDbContext db) : IUserService
{
    private readonly ApplicationDbContext _db = db;
    private readonly PasswordHasher<AppUser> _hasher = new();

    public Task<AppUser?> FindByEmailAsync(string email, CancellationToken ct) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<AppUser> CreateAsync(string email, string password, string role, string? fullName, Guid? clinicId, CancellationToken ct)
    {
        var user = new AppUser
        {
            Email = email,
            Role = role,
            FullName = fullName,
            ClinicId = clinicId
        };
        user.PasswordHash = _hasher.HashPassword(user, password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    public bool VerifyPassword(AppUser user, string password) =>
        _hasher.VerifyHashedPassword(user, user.PasswordHash, password)
            is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
}