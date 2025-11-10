using Microsoft.AspNetCore.Mvc;
using ang_emp_api.Models;
using ang_emp_api.Data;
using ang_emp_api.DTOs;
using MailKit.Net.Smtp;
using MimeKit;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace ang_emp_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public EmployeeController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required.");

            if (_context.Employees.Any(e => e.Email == dto.Email))
                return BadRequest("Email already exists");

            var otp = GenerateOtp();
            var employee = new Employee
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Password = dto.Password,

                Mobile = dto.Mobile,
                Address = dto.Address,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,

                // Assign DepartmentId instead of Department object
                DepartmentId = dto.DepartmentId,

                Designation = dto.Designation,
                JoiningDate = dto.JoiningDate ?? DateTime.Now,
                Salary = dto.Salary,

                isverify = false,
                OTP = otp,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            try
            {
                await SendOtpEmailAsync(employee.Email, otp);
            }
            catch (Exception ex)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                return StatusCode(500, "Failed to send OTP email: " + ex.Message);
            }

            return Ok(new { Message = "OTP sent to your email.", Email = employee.Email });
        }


        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var existing = await _context.Employees.FirstOrDefaultAsync(e => e.Email == dto.Email && e.OTP == dto.OTP);
            if (existing == null)
                return BadRequest("Invalid email or OTP.");

            existing.isverify = true;
            existing.OTP = "";
            await _context.SaveChangesAsync();

            return Ok("Email verified successfully.");
        }

        

        #region Helpers

        private string GenerateOtp()
        {
            var rnd = new Random();
            return rnd.Next(100000, 999999).ToString();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Email and password are required.");

            // Find user by email+password and must be verified
            var user = await _context.Employees.FirstOrDefaultAsync(e =>
                e.Email == dto.Email &&
                e.Password == dto.Password &&
                e.isverify);

            if (user == null)
                return BadRequest("Invalid credentials or email not verified.");

            // create a new session id (single active session)
            var sessionId = Guid.NewGuid().ToString();
            user.SessionId = sessionId;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user, sessionId);

            return Ok(new
            {
                token,
                role = user.Role,
                expiresIn = int.Parse(_configuration["Jwt:ExpiresMinutes"]) * 60
            });
        }
        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idClaim == null) return Unauthorized();

            var user = await _context.Employees.FindAsync(int.Parse(idClaim));
            if (user == null) return Unauthorized();

            user.SessionId = null; // invalidate session
            await _context.SaveChangesAsync();
            return Ok("Logged out");
        }
        private string GenerateJwtToken(Employee user, string sessionId)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSection["Key"]);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("sessionId", sessionId)
            };

            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSection["ExpiresMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private async Task SendOtpEmailAsync(string email, string otp)
        {
            var smtpSection = _configuration.GetSection("Smtp");
            var host = smtpSection["Host"];
            var port = int.Parse(smtpSection["Port"] ?? "587");
            var user = smtpSection["User"];
            var pass = smtpSection["Pass"];
            var fromName = smtpSection["FromName"];
            var fromEmail = smtpSection["FromEmail"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = "OTP Verification";
            message.Body = new TextPart("plain") { Text = $"Your OTP is {otp}" };

            using var client = new SmtpClient();
            // Use StartTls (recommended)
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(user, pass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        #endregion
        // Update/Edit Employee
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound("Employee not found");

            employee.FullName = dto.FullName;
            employee.Mobile = dto.Mobile;
            employee.Address = dto.Address;
            employee.Gender = dto.Gender;
            employee.DateOfBirth = dto.DateOfBirth ?? employee.DateOfBirth;

            // ✅ Set DepartmentId, not Department
            employee.DepartmentId = dto.DepartmentId;

            employee.Designation = dto.Designation;
            employee.JoiningDate = dto.JoiningDate ?? employee.JoiningDate;
            employee.Salary = dto.Salary ?? employee.Salary;

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Employee updated successfully", Employee = employee });
        }


        // Delete Employee
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound("Employee not found");

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Employee deleted successfully" });
        }

        // Get all employees (if not already present)
        [HttpGet("list")]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .ToListAsync();
       
            return Ok(employees);
        }
        [HttpGet("list-paged")]
        public async Task<IActionResult> GetEmployeesPaged(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? search = null)
        {
            var query = _context.Employees.Include(e => e.Department).AsQueryable();

            // Filter by search text (searching name, email, designation)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(e =>
                    e.FullName.ToLower().Contains(search) ||
                    e.Email.ToLower().Contains(search) ||
                    e.Designation.ToLower().Contains(search));
            }

            var totalRecords = await query.CountAsync();

            var employees = await query
                .OrderBy(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = employees,
                page,
                pageSize,
                totalRecords,
                totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            });
        }

    }
}
