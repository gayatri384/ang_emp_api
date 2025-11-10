using ang_emp_api.Data;
using ang_emp_api.DTOs;
using Microsoft.AspNetCore.Mvc;
using ang_emp_api.Models;
using Microsoft.EntityFrameworkCore;
using ang_emp_api.Services;
using System.Security.Claims;

namespace ang_emp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public AssetController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        //  Get All Assets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssetDto>>> GetAssets()
        {
            var assets = await _context.Assets
                .Select(a => new AssetDto
                {
                    Id = a.Id,
                    AssetName = a.AssetName,
                    Description = a.Description,
                    SerialNumber = a.SerialNumber,
                    Category = a.Category,
                    PurchaseDate = a.PurchaseDate,
                    IsAvailable = a.IsAvailable,
                    IsDamaged = a.IsDamaged
                })
                .ToListAsync();

            return Ok(assets);
        }

        //  Add Asset
        [HttpPost]
        public async Task<ActionResult> AddAsset(AssetDto dto)
        {
            var asset = new Asset
            {
                AssetName = dto.AssetName,
                Description = dto.Description,
                SerialNumber = dto.SerialNumber,
                Category = dto.Category,
                IsAvailable = true,
                IsDamaged = dto.IsDamaged
            };

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Asset added successfully!" });
        }

        //  Update Asset
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsset(int id, AssetDto dto)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
                return NotFound(new { message = "Asset not found" });

            asset.AssetName = dto.AssetName;
            asset.Description = dto.Description;
            asset.SerialNumber = dto.SerialNumber;
            asset.Category = dto.Category;
            asset.IsDamaged = dto.IsDamaged;
            asset.IsAvailable = dto.IsAvailable;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Asset updated successfully!" });
        }

        //  Delete Asset
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsset(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
                return NotFound(new { message = "Asset not found" });

            if (!asset.IsAvailable)
                return BadRequest(new { message = "Cannot delete asset while assigned to an employee." });

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Asset deleted successfully!" });
        }

        // Assign Asset to Employee
        [HttpPost("assign")]
        public async Task<ActionResult> AssignAsset(AssignAssetRequestDto dto)
        {
            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            var asset = await _context.Assets.FindAsync(dto.AssetId);

            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            if (asset == null)
                return NotFound(new { message = "Asset not found" });

            if (!asset.IsAvailable)
                return BadRequest(new { message = "Asset is already assigned." });

            var assignment = new AssetAssignment
            {
                EmployeeId = employee.Id,
                AssetId = asset.Id,
                Remarks = dto.Remarks,
                AssignedDate = DateTime.Now,
                IsDamagedOnReturn = dto.IsDamagedOnReturn
            };

            _context.AssetAssignments.Add(assignment);
            asset.IsAvailable = false;
            await _context.SaveChangesAsync();

            // Send Email
            string damageStatus = dto.IsDamagedOnReturn ? "⚠️ Damaged" : "✅ Good Condition";
            string body = $@"
                <h2>Asset Assignment Notification</h2>
                <p>Dear {employee.FullName},</p>
                <p>The following asset has been assigned to you:</p>
                <ul>
                    <li><strong>Asset:</strong> {asset.AssetName}</li>
                    <li><strong>Serial Number:</strong> {asset.SerialNumber}</li>
                    <li><strong>Status:</strong> {damageStatus}</li>
                    <li><strong>Remarks:</strong> {dto.Remarks}</li>
                    <li><strong>Assigned Date:</strong> {DateTime.Now:dd MMM yyyy}</li>
                </ul>
                <p>Please confirm receipt of this asset.</p>";

            await _emailService.SendEmailAsync(employee.Email, "Asset Assigned to You", body);

            return Ok(new { message = "Asset assigned successfully and email sent." });
        }

        //  Get All Assignments
        [HttpGet("assignments")]
        public async Task<ActionResult<IEnumerable<AssetAssignmentDto>>> GetAssignments()
        {
            var assignments = await _context.AssetAssignments
                .Include(a => a.Employee)
                .Include(a => a.Asset)
                .Select(a => new AssetAssignmentDto
                {
                    AssignmentId = a.AssignmentId,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.Employee.FullName,
                    EmployeeEmail = a.Employee.Email,
                    AssetId = a.AssetId,
                    AssetName = a.Asset.AssetName,
                    AssignedDate = a.AssignedDate,
                    ReturnDate = a.ReturnDate,
                    IsDamagedOnReturn = a.IsDamagedOnReturn,
                    Remarks = a.Remarks
                })
                .ToListAsync();

            return Ok(assignments);
        }

        //  Update Asset Assignment
        [HttpPut("assign/{assignmentId}")]
        public async Task<ActionResult> UpdateAssignment(int assignmentId, AssignAssetRequestDto dto)
        {
            var assignment = await _context.AssetAssignments.FindAsync(assignmentId);
            if (assignment == null)
                return NotFound(new { message = "Assignment not found" });

            assignment.Remarks = dto.Remarks;
            assignment.IsDamagedOnReturn = dto.IsDamagedOnReturn;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Asset assignment updated successfully!" });
        }

        //  Return Assigned Asset (mark as returned)
        [HttpPut("return/{assignmentId}")]
        public async Task<ActionResult> ReturnAsset(int assignmentId, [FromBody] bool isDamaged)
        {
            var assignment = await _context.AssetAssignments
                .Include(a => a.Asset)
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);

            if (assignment == null)
                return NotFound(new { message = "Assignment not found" });

            assignment.ReturnDate = DateTime.Now;
            assignment.IsDamagedOnReturn = isDamaged;
            assignment.Asset.IsAvailable = true;
            assignment.Asset.IsDamaged = isDamaged;

            await _context.SaveChangesAsync();

            string damageStatus = isDamaged ? "⚠️ Damaged" : "✅ Good Condition";
            string body = $@"
                <h2>Asset Return Notification</h2>
                <p>Dear {assignment.Employee.FullName},</p>
                <p>Your asset '{assignment.Asset.AssetName}' has been marked as returned.</p>
                <p>Status: {damageStatus}</p>
                <p>Thank you for returning the asset.</p>";

            await _emailService.SendEmailAsync(assignment.Employee.Email, "Asset Return Confirmation", body);

            return Ok(new { message = "Asset return recorded and email sent." });
        }

        //  Delete Asset Assignment Record
        [HttpDelete("assign/{assignmentId}")]
        public async Task<ActionResult> DeleteAssignment(int assignmentId)
        {
            var assignment = await _context.AssetAssignments.FindAsync(assignmentId);
            if (assignment == null)
                return NotFound(new { message = "Assignment not found" });

            _context.AssetAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Asset assignment deleted successfully!" });
        }

        // my assets
        [HttpGet("my-assets")]
        public async Task<IActionResult> GetMyAssets()
        {
            //  Extract employee ID from JWT token
            var empIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (empIdClaim == null)
            {
                return Unauthorized("Invalid or missing token.");
            }

            int employeeId = int.Parse(empIdClaim);

            // Fetch only the assets assigned to the logged-in employee
            var assets = await _context.AssetAssignments
                .Where(a => a.EmployeeId == employeeId)
                .OrderByDescending(a => a.AssetId)
                .Select(a => new
                {
                    a.AssetId,
                    AssetName = a.Asset.AssetName,  
                    a.Remarks,
                    a.AssignedDate,
                    a.IsDamagedOnReturn
                })
                .ToListAsync();
            return Ok(assets);
        }

        // pagination and search
        [HttpGet("list-paged")]
        public async Task<IActionResult> GetAssetsList(
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? search = null)
        {
            // Base query
            var query = _context.Assets.AsQueryable();

            // Apply Search filter
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(a =>
                    a.AssetName.ToLower().Contains(search) ||
                    a.Description.ToLower().Contains(search) ||
                    a.Category.ToLower().Contains(search) ||
                    a.SerialNumber.ToLower().Contains(search)
                );
            }

            // Get total count (for pagination info)
            var totalRecords = await query.CountAsync();

            //  Apply pagination
            var assets = await query
                .OrderByDescending(a => a.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AssetDto
                {
                    Id = a.Id,
                    AssetName = a.AssetName,
                    Description = a.Description,
                    SerialNumber = a.SerialNumber,
                    Category = a.Category,
                    PurchaseDate = a.PurchaseDate,
                    IsAvailable = a.IsAvailable,
                    IsDamaged = a.IsDamaged
                })
                .ToListAsync();

            //  Return with pagination metadata
            return Ok(new
            {
                data = assets,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalRecords = totalRecords,
                    totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
                }
            });

        }
    }
}

