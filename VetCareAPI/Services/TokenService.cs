using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VetCareAPI.Data;
using VetCareAPI.Models;

namespace VetCareAPI.Services;

public interface ITokenService
{
    (string accessToken, DateTime expiresAtUtc) CreateAccessToken(AppUser user);
    Task<string> IssueRefreshTokenAsync(AppUser user, string? device, string? ip, CancellationToken ct);
    Task<(AppUser? user, RefreshToken? token)> ValidateRefreshTokenAsync(string refreshToken, CancellationToken ct);
    Task<string> RotateRefreshTokenAsync(RefreshToken oldToken, CancellationToken ct);
    string GenerateRefreshTokenPlaintext();
    string Hash(string input);
}

public class TokenService(IOptions<JwtOptions> jwtOptions, ApplicationDbContext db) : ITokenService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;
    private readonly ApplicationDbContext _db = db;

    public (string accessToken, DateTime expiresAtUtc) CreateAccessToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_jwt.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.UniqueName, user.Email),
            new("role", user.Role)
        };

        if (user.ClinicId.HasValue)
            claims.Add(new Claim("clinic_id", user.ClinicId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return (jwt, expires);
    }

    public string GenerateRefreshTokenPlaintext()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    public async Task<string> IssueRefreshTokenAsync(AppUser user, string? device, string? ip, CancellationToken ct)
    {
        var plain = GenerateRefreshTokenPlaintext();
        var hash = Hash(plain);

        var rt = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hash,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            Device = device,
            IpAddress = ip
        };

        _db.RefreshTokens.Add(rt);
        await _db.SaveChangesAsync(ct);
        return plain; // return plaintext once to client
    }

    public async Task<(AppUser? user, RefreshToken? token)> ValidateRefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        var hash = Hash(refreshToken);
        var token = await _db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (token is null)                 return (null, null);
        if (token.ExpiresAtUtc <= DateTime.UtcNow) return (null, null);
        if (token.RevokedAtUtc != null)    return (null, null);
        // IMPORTANT: already rotated (single-use)
        if (!string.IsNullOrEmpty(token.ReplacedByTokenHash)) return (null, null);

        return (token.User, token);
    }

    public async Task<string> RotateRefreshTokenAsync(RefreshToken oldToken, CancellationToken ct)
    {
        var newPlain = GenerateRefreshTokenPlaintext();
        var newHash  = Hash(newPlain);

        oldToken.RevokedAtUtc = DateTime.UtcNow;         // mark old as revoked
        oldToken.ReplacedByTokenHash = newHash;          // link to the new one

        var newRt = new RefreshToken
        {
            UserId = oldToken.UserId,
            TokenHash = newHash,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
            Device = oldToken.Device,
            IpAddress = oldToken.IpAddress
        };

        _db.RefreshTokens.Add(newRt);
        await _db.SaveChangesAsync(ct);
        return newPlain;
    }
}
