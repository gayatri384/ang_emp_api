using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ang_emp_api.Models;
using ang_emp_api.Data;
using System.Threading.Tasks;

namespace ang_emp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetDashboardOverview()
        {
            var totalEmployee = await _context.Employees.CountAsync();
            var activeDepartment = await _context.Departments.CountAsync(d => d.IsActive == true);
            var totalAssets = await _context.Assets.CountAsync();
            var pendingLeaves = await _context.Leaves.CountAsync(l => l.Status == "pending");

            return Ok(new
            {
                totalEmployee,
                activeDepartment,
                totalAssets,
                pendingLeaves
            });
        }
    }
}
