using ang_emp_api.Data;
using ang_emp_api.DTOs;
using ang_emp_api.Models;
using ang_emp_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace ang_emp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly AttendanceService _service;

        public AttendanceController(AttendanceService service)
        {
            _service = service;
        }

        //  Helper: Get Employee ID from JWT Claims
        private int? GetCurrentUserEmployeeId()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                // Correct claim key used by JWT (NameIdentifier)
                var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                if (claim != null && int.TryParse(claim.Value, out var id))
                    return id;
            }
            return null;
        }

        //  Employee marks attendance
        [HttpPost("mark")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> Mark([FromBody] AttendanceCreateDto dto)
        {
            var employeeId = GetCurrentUserEmployeeId();
            if (employeeId == null)
                return Unauthorized("Invalid or missing employee ID in token.");

            var result = await _service.MarkAttendanceAsync(employeeId.Value, dto);
            return Ok(result);
        }

        //  Employee: get own attendance range
        [HttpGet("me")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> GetMine([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var employeeId = GetCurrentUserEmployeeId();
            if (employeeId == null)
                return Unauthorized("Invalid or missing employee ID in token.");

            var list = await _service.GetAttendancesForEmployeeAsync(employeeId.Value, from, to);
            return Ok(list);
        }

        //  Admin: get attendance for a date
        [HttpGet("admin/date")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetForDate([FromQuery] DateTime date)
        {
            var list = await _service.GetAttendancesForAdminAsync(date);
            return Ok(list);
        }

        //  Admin: create or update attendance
        [HttpPost("admin/upsert")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpsertAdmin([FromBody] AttendanceUpdateDto dto)
        {
            var adminName = User?.Identity?.Name ?? "admin";
            var result = await _service.UpsertAttendanceAdminAsync(dto, adminName);
            return Ok(result);
        }

        //  Admin: Get all attendance with employee name + optional employee filter
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAttendance([FromQuery] int? employeeId = null, [FromQuery] string? employeeName = null)
        {
            // Load all attendance with Employee info
            var query = _service.GetAllAttendanceQueryable();

            //  Optional filters
            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeId == employeeId.Value);

            if (!string.IsNullOrEmpty(employeeName))
                query = query.Where(a => a.Employee.FullName.Contains(employeeName));

            // Fetch & shape response
            var result = await query
                .OrderByDescending(a => a.Date)
                .Select(a => new
                {
                    a.Id,
                    a.EmployeeId,
                    EmployeeName = a.Employee.FullName,
                    a.Date,
                    a.CheckIn,
                    a.CheckOut,
                    a.Status,
                    a.Remarks
                })
                .ToListAsync();

            return Ok(result);
        }
    }

}
