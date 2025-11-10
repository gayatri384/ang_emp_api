using ang_emp_api.DTOs;
namespace ang_emp_api.DTOs
{
    public class UpdateEmployeeDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public int? DepartmentId { get; set; }
        public string Designation { get; set; } = string.Empty;
        public DateTime? JoiningDate { get; set; }
        public decimal? Salary { get; set; }
    }
}
