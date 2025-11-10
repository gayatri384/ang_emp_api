using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace ang_emp_api.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string DepartmentName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        
        public ICollection<Employee>? Employees { get; set; }
    }
}
