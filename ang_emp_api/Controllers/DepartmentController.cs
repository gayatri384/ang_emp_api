using ang_emp_api.Data;
using ang_emp_api.DTOs;
using Microsoft.AspNetCore.Mvc;
using ang_emp_api.Models;
using Microsoft.EntityFrameworkCore;

namespace ang_emp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : Controller
    {
        private readonly AppDbContext _context;

        public DepartmentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Department
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Where(d => d.IsActive)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    DepartmentName = d.DepartmentName,
                    Description = d.Description
                }).ToListAsync();

            return Ok(departments);
        }

        // GET: api/Department/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null || !department.IsActive)
                return NotFound("Department not found");

            var dto = new DepartmentDto
            {
                Id = department.Id,
                DepartmentName = department.DepartmentName,
                Description = department.Description
            };

            return Ok(dto);
        }

        // ✅ PAGINATION + SEARCH
        [HttpGet("list-paged")]
        public async Task<IActionResult> GetDepartmentsPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            // Base query — only active departments
            var query = _context.Departments
                .Where(d => d.IsActive)
                .AsQueryable();
                
            // Apply search filter (on name or description)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(d =>
                    d.DepartmentName.ToLower().Contains(search) ||
                    (d.Description != null && d.Description.ToLower().Contains(search))
                );
            }

            // Count total records
            var totalRecords = await query.CountAsync();

            //  Apply pagination
            var departments = await query
                .OrderBy(d => d.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    DepartmentName = d.DepartmentName,
                    Description = d.Description
                })
                .ToListAsync();

            // Return paginated response
            return Ok(new
            {
                data = departments,
                page,
                pageSize,
                totalRecords,
                totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            });
        }

        // POST: api/Department
        [HttpPost("add")]
        public async Task<IActionResult> AddDepartment([FromBody] CreateDepartmentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.DepartmentName))
                return BadRequest("Department name is required.");

            // Check if department already exists
            if (_context.Departments.Any(d => d.DepartmentName == dto.DepartmentName && d.IsActive))
                return BadRequest("Department already exists.");

            var department = new Department
            {
                DepartmentName = dto.DepartmentName,
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Department added successfully", Department = department });
        }

        // PUT: api/Department/update/5
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto dto)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null || !department.IsActive)
                return NotFound("Department not found");

            department.DepartmentName = dto.DepartmentName;
            department.Description = dto.Description;
            department.UpdatedAt = DateTime.Now;

            _context.Departments.Update(department);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Department updated successfully", Department = department });
        }

        // DELETE: api/Department/delete/5
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null || !department.IsActive)
                return NotFound("Department not found");

            // Optional: Check if department has employees
            if (department.Employees != null && department.Employees.Any())
                return BadRequest("Cannot delete department with assigned employees.");

            // Soft delete
            department.IsActive = false;
            department.UpdatedAt = DateTime.Now;

            _context.Departments.Update(department);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Department deleted successfully" });
        }

    }
}
