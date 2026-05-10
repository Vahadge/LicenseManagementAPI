using LicenseManagementAPI.Application.DTOs;
using LicenseManagementAPI.Application.Interfaces;
using LicenseManagementAPI.Common;
using Microsoft.AspNetCore.Mvc;

namespace LicenseManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _service;

    public DoctorsController(IDoctorService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DoctorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] DoctorListRequest request)
    {
        var result = await _service.GetDoctorsAsync(request);
        return Ok(ApiResponse<PagedResult<DoctorDto>>.Success(result));
    }

    [HttpGet("expired")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DoctorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpired()
    {
        var doctors = await _service.GetExpiredDoctorsAsync();
        return Ok(ApiResponse<IEnumerable<DoctorDto>>.Success(doctors));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DoctorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var doctor = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<DoctorDto>.Success(doctor));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DoctorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateDoctorRequest request)
    {
        var doctor = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = doctor.Id },
            ApiResponse<DoctorDto>.Success(doctor, "Doctor created successfully."));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<DoctorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDoctorRequest request)
    {
        var doctor = await _service.UpdateAsync(id, request);
        return Ok(ApiResponse<DoctorDto>.Success(doctor, "Doctor updated successfully."));
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        await _service.UpdateStatusAsync(id, request);
        return Ok(ApiResponse<object>.Success(null, "Doctor status updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<object>.Success(null, "Doctor deleted successfully."));
    }
}
