using ang_emp_api.Data;
using ang_emp_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ang_emp_api.DTOs;
using System.Security.Claims;

namespace ang_emp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkTasksController : Controller
    {
        private readonly AppDbContext _context;
        public WorkTasksController(AppDbContext context)
        {
            _context = context;
        }

        // Helper Method: Get Logged-in Employee ID from JWT
        private int? GetCurrentUserEmployeeId()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (claim != null && int.TryParse(claim.Value, out var id))
                    return id;
            }
            return null;
        }

        // ================= ADMIN PANEL =================

        // Admin: Get all tasks
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _context.WorkTasks
                .Include(t => t.AssignedTo)
                .Include(t => t.Column)
                .OrderBy(t => t.Column.Order)
                .ThenBy(t => t.Order)
                .ToListAsync();

            return Ok(tasks);
        }

        // Admin: Get tasks by employee ID (filter dropdown)
        [HttpGet("admin/by-employee/{empId}")]
        public async Task<IActionResult> GetByEmployee(int empId)
        {
            var tasks = await _context.WorkTasks
                .Where(t => t.AssignedToId == empId)
                .Include(t => t.AssignedTo)
                .Include(t => t.Column)
                .OrderBy(t => t.Order)
                .ToListAsync();

            return Ok(tasks);
        }

        // ================= EMPLOYEE PANEL =================

        [HttpGet("employee")]
        public async Task<IActionResult> GetEmployeeTasks()
        {
            var empId = GetCurrentUserEmployeeId();
            if (empId == null)
                return Unauthorized("Invalid or missing JWT token");

            var tasks = await _context.WorkTasks
                .Where(t => t.AssignedToId == empId)
                .Include(t => t.Column)
                .OrderBy(t => t.Order)
                .ToListAsync();

            return Ok(tasks);
        }

        // ================= CRUD TASKS =================

        [HttpPost("create")]
        public async Task<IActionResult> CreateTask(CreateTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);  // <-- returns EXACT missing fields

            var task = new WorkTask
            {
                Title = dto.Title,
                Description = dto.Description,
                AssignedToId = dto.AssignedToId,
                ColumnId = dto.ColumnId,
                Order = dto.Order,
                Priority = dto.Priority,
                DueDate = dto.DueDate
            };

            _context.WorkTasks.Add(task);
            await _context.SaveChangesAsync();

            return Ok(task);
        }


        [HttpPut("update")]
        public async Task<IActionResult> UpdateTask(UpdateTaskDto dto)
        {
            var task = await _context.WorkTasks.FindAsync(dto.Id);
            if (task == null) return NotFound();

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.AssignedToId = dto.AssignedToId;
            task.Priority = dto.Priority;
            task.DueDate = dto.DueDate;

            await _context.SaveChangesAsync();
            return Ok(task);
        }
            
        [HttpPost("move")]
        public async Task<IActionResult> MoveTask(MoveTaskDto dto)
        {
            var task = await _context.WorkTasks.FindAsync(dto.TaskId);
            if (task == null) return NotFound("Task not found!");

            task.ColumnId = dto.ToColumnId;
            task.Order = dto.NewOrder;

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
