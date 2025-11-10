using ang_emp_api.DTOs;
namespace ang_emp_api.DTOs
{
    public class RegisterDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }   // must match JSON
        public string Password { get; set; }

        public string? Mobile { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public int? DepartmentId { get; set; }
        public string? Designation { get; set; }
        public DateTime? JoiningDate { get; set; }
        public decimal? Salary { get; set; }
    }
}
