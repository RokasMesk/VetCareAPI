using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCareAPI.Models;
using VetCareAPI.Models.DTOs.Clinics;
using VetCareAPI.Models.DTOs.Visits;
using VetCareAPI.Services;

namespace VetCareAPI.Controllers;

[Route("api/clinics")]
[ApiController]
public class ClinicsController : ControllerBase
{
    private readonly ClinicService _clinicService;
    public ClinicsController(ClinicService clinicService) => _clinicService = clinicService;
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll() => Ok(await _clinicService.GetAllAsync());

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var clinic = await _clinicService.GetAsync(id);
        if (clinic is null)
        {
            return NotFound();
        }
        return Ok(clinic);
        
    }
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateClinicDto dto)
    {
        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        var created = await _clinicService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.ClinicStaff}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClinicDto dto)
    {
        if (!ModelState.IsValid) return UnprocessableEntity(ModelState);

        if (!User.IsInRole(Roles.Admin))
        {
            var staffClinicIdClaim = User.FindFirst("clinic_id")?.Value;
            if (!Guid.TryParse(staffClinicIdClaim, out var staffClinicId) || staffClinicId != id)
                return Forbid();
        }

        return await _clinicService.UpdateAsync(id, dto) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.ClinicStaff}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!User.IsInRole(Roles.Admin))
        {
            var staffClinicIdClaim = User.FindFirst("clinic_id")?.Value;
            if (!Guid.TryParse(staffClinicIdClaim, out var staffClinicId) || staffClinicId != id)
                return Forbid();
        }
        return await _clinicService.DeleteAsync(id) ? NoContent() : NotFound();
    }
    
    [HttpGet("{id:guid}/visits")]
    [ProducesResponseType(typeof(List<VisitDto>), 200)]
    [ProducesResponseType(404)]
    [Authorize(Roles = $"{Roles.Admin},{Roles.ClinicStaff}")]
    public async Task<IActionResult> GetVisits(Guid id, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc)
    {
        var clinic = await _clinicService.GetAsync(id);
        if (clinic is null) return NotFound();
        var data = await _clinicService.GetClinicVisitsAsync(id, fromUtc, toUtc);
        return Ok(data);
    }
}