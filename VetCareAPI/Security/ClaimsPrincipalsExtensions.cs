
using System.Security.Claims;

namespace VetCareAPI.Security
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user) =>
            Guid.Parse(
                user.FindFirstValue(ClaimTypes.NameIdentifier)         // present in your token
                ?? user.FindFirstValue("sub")!                         // fallback if you ever switch to "sub"
            );
    }
}