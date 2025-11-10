using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ang_emp_api.Models
{
    public class AssetAssignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public int AssetId { get; set; }

        [ForeignKey("AssetId")]
        public Asset Asset { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.Now;

        public DateTime? ReturnDate { get; set; }

        public bool IsDamagedOnReturn { get; set; } = false;

        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
