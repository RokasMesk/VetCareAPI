using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetCareAPI.Models;
using VetCareAPI.Models.DTOs.Visits;
using VetCareAPI.Services;

namespace VetCareAPI.Controllers;

[ApiController]
[Route("api/visits")]
[Produces("application/json")]
[Authorize]
public class VisitsController : ControllerBase
{
    private readonly VisitService _visitService;
    public VisitsController(VisitService svc) => _visitService = svc;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var visit = await _visitService.GetAsync(id);
        if (visit is null)
        {
            return NotFound();
        }
        return Ok(visit);
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.User},{Roles.ClinicStaff},{Roles.Admin}")]
    public async Task<IActionResult> Create([FromBody] CreateVisitDto dto)
    {
        if (!ModelState.IsValid) return UnprocessableEntity(ModelState);
        try
        {
            var created = await _visitService.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)          { return UnprocessableEntity(new { message = ex.Message }); }
        catch (InvalidOperationException ex)  { return UnprocessableEntity(new { message = ex.Message }); }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.User},{Roles.ClinicStaff},{Roles.Admin}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVisitDto dto)
    {
        if (!ModelState.IsValid) return UnprocessableEntity(ModelState);
        try
        {
            return await _visitService.UpdateAsync(id, dto) ? NoContent() : NotFound();
        }
        catch (ArgumentException ex) { return UnprocessableEntity(new { message = ex.Message }); }
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{Roles.User},{Roles.ClinicStaff},{Roles.Admin}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _visitService.DeleteAsync(id) ? NoContent() : NotFound();
    

    [HttpGet("pet/{petId:guid}")]
    [Authorize(Roles = $"{Roles.User},{Roles.Admin},{Roles.ClinicStaff}")]
    public async Task<IActionResult> GetByPetAsync(Guid petId, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc)
    {
        var petVisits = await _visitService.GetByPetAsync(petId, fromUtc, toUtc);
        return Ok(petVisits);
    }

    [HttpGet("clinic/{clinicId:guid}")]
    [Authorize(Roles = $"{Roles.ClinicStaff},{Roles.Admin}")]
    public async Task<IActionResult> GetByClinic(Guid clinicId, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc)
        => Ok(await _visitService.GetByClinicAsync(clinicId, fromUtc, toUtc));
}
