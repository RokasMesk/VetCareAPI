using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCareAPI.Services;
using VetCareAPI.Models;

namespace VetCareAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ITokenService tokens, IUserService users) : ControllerBase
{
    // DTOs
    public record RegisterDto(string Email, string Password, string? FullName);
    public record LoginDto(string Email, string Password);
    public record TokenResponse(string AccessToken, DateTime ExpiresAtUtc, string RefreshToken);
    public record RefreshRequest(string RefreshToken);
    public record InviteAcceptDto(string InviteToken, string Password); // if you later add invites

    // Admin: create clinic staff
    public record CreateClinicStaffDto(
        string Email,
        string Password,
        string? FullName,
        Guid ClinicId
    );

    /// <summary>
    /// Public: Owner self-registration
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        var existing = await users.FindByEmailAsync(dto.Email, ct);
        if (existing != null) return Conflict(new { error = "Email already registered" });

        var user = await users.CreateAsync(dto.Email, dto.Password, Roles.User, dto.FullName, clinicId: null, ct);

        var (access, exp) = tokens.CreateAccessToken(user);
        var refresh = await tokens.IssueRefreshTokenAsync(user, Request.Headers.UserAgent.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), ct);

        return Ok(new TokenResponse(access, exp, refresh));
    }

    /// <summary>
    /// Public: login by email + password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(dto.Email, ct);
        if (user == null || !users.VerifyPassword(user, dto.Password))
            return Unauthorized(new { error = "Invalid credentials" });

        var (access, exp) = tokens.CreateAccessToken(user);
        var refresh = await tokens.IssueRefreshTokenAsync(user, Request.Headers.UserAgent.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), ct);

        return Ok(new TokenResponse(access, exp, refresh));
    }

    /// <summary>
    /// Public: refresh flow (rotating refresh tokens)
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest dto, CancellationToken ct)
    {
        var (user, oldToken) = await tokens.ValidateRefreshTokenAsync(dto.RefreshToken, ct);
        if (user is null || oldToken is null)
            return Unauthorized(new { error = "Invalid or expired refresh token" });

        // Rotate refresh token
        var newRefresh = await tokens.RotateRefreshTokenAsync(oldToken, ct);

        var (access, exp) = tokens.CreateAccessToken(user);
        return Ok(new TokenResponse(access, exp, newRefresh));
    }

    /// <summary>
    /// Optional: simple logout -> revoke current refresh token by plaintext
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest dto, CancellationToken ct)
    {
        var (user, token) = await tokens.ValidateRefreshTokenAsync(dto.RefreshToken, ct);
        if (user is null || token is null) return Ok();

        token.RevokedAtUtc = DateTime.UtcNow;
        await HttpContext.RequestServices.GetRequiredService<VetCareAPI.Data.ApplicationDbContext>().SaveChangesAsync(ct);
        return Ok();
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin-check")]
    public IActionResult AdminCheck() => Ok(new { ok = true, role = Roles.Admin });

    /// <summary>
    /// Admin-only: create clinic staff user for a specific clinic
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost("create-clinic-staff")]
    public async Task<IActionResult> CreateClinicStaff([FromBody] CreateClinicStaffDto dto, CancellationToken ct)
    {
        // ensure clinic exists
        var db = HttpContext.RequestServices.GetRequiredService<VetCareAPI.Data.ApplicationDbContext>();
        var clinic = await db.Clinics.FindAsync([dto.ClinicId], ct);
        if (clinic is null)
            return NotFound(new { error = "Clinic not found" });

        // email uniqueness
        var existing = await users.FindByEmailAsync(dto.Email, ct);
        if (existing is not null)
            return Conflict(new { error = "Email already registered" });

        var user = await users.CreateAsync(dto.Email, dto.Password, Roles.ClinicStaff, dto.FullName, dto.ClinicId, ct);

        // you can later swap to a slimmer DTO if you want
        return Ok(new { user.Id, user.FullName, user.Email, user.Role, user.ClinicId });
    }
}
