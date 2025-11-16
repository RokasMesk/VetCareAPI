using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VetCareAPI.Controllers;

[ApiController]
[Route("api/debug")]
public class DebugController : ControllerBase
{
    [HttpGet("me")][Authorize]
    public IActionResult Me() => Ok(new {
        Authenticated = User.Identity?.IsAuthenticated ?? false,
        Claims = User.Claims.Select(c => new { c.Type, c.Value }),
        InRole = new {
            Admin = User.IsInRole("Admin"),
            ClinicStaff = User.IsInRole("ClinicStaff"),
            UserRole = User.IsInRole("User")
        }
    });
}