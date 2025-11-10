using ang_emp_api.Data;
using ang_emp_api.DTOs;
using ang_emp_api.Models;
using ang_emp_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ang_emp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public LeaveController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> ApplyLeave([FromBody] LeaveRequestDto dto)
        {
            var emp = await _context.Employees.FindAsync(dto.EmployeeId);
            if (emp == null) return NotFound("Employee not found");

            var leave = new Leave
            {
                EmployeeId = dto.EmployeeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Reason = dto.Reason,
                Status = "Pending"
            };

            _context.Leaves.Add(leave);
            await _context.SaveChangesAsync();

            // ✅ Send mail to HR/company
            var subject = $"New Leave Application from {emp.FullName}";
            var body = $@"
                <p>Dear HR,</p>
                <p>{emp.FullName} has applied for leave from 
                <b>{dto.StartDate:dd-MMM-yyyy}</b> to <b>{dto.EndDate:dd-MMM-yyyy}</b>.</p>
                <p>Reason: {dto.Reason}</p>
                <p>Status: Pending</p>
                <p><br/>Best Regards,<br/>Asset Management System</p>";

            // Replace with your HR/company email
            await _emailService.SendEmailAsync("gayatri384chauhan@gmail.com", subject, body);

            return Ok(new { message = "Leave request submitted successfully and email sent to HR" });
        }

        [HttpPost("update-status")]
        public async Task<IActionResult> UpdateStatus([FromBody] LeaveUpdateDto dto)
        {
            var leave = await _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == dto.LeaveId);

            if (leave == null) return NotFound("Leave not found");

            leave.Status = dto.Status;
            leave.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // ✅ Send email to employee
            var subject = $"Leave Status Update - {dto.Status}";
            var body = $@"
                <p>Dear {leave.Employee.FullName},</p>
                <p>Your leave request from <b>{leave.StartDate:dd-MMM-yyyy}</b> to 
                <b>{leave.EndDate:dd-MMM-yyyy}</b> has been 
                <b style='color:blue;'>{dto.Status}</b>.</p>
                <p>Reason: {leave.Reason}</p>
                <p><br/>Best Regards,<br/>HR Team</p>";

            await _emailService.SendEmailAsync(leave.Employee.Email, subject, body);

            //return Ok($"Leave status updated to {dto.Status} and email sent to employee");
            return Ok(new { message = $"Leave status updated to {dto.Status} and email sent to employee" });
 
        }
        // get my leaves
        [HttpGet("my-leaves")]
        public async Task<IActionResult> GetMyLeaves()
        {
            // Extract employeeId from JWT claim
            var empIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (empIdClaim == null)
                return Unauthorized("Invalid or missing token.");

            int employeeId = int.Parse(empIdClaim);

            var leaves = await _context.Leaves
                .Where(l => l.EmployeeId == employeeId)
                .OrderByDescending(l => l.StartDate)
                .ToListAsync();

            return Ok(leaves);
        }


        // Get all leaves (Admin/HR)
        [HttpGet]
        public async Task<IActionResult> GetLeaves()
        {
            var leaves = await _context.Leaves
                .Include(l => l.Employee)
                .ToListAsync();

            return Ok(leaves);
        }
    }
}
