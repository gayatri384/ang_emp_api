using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ang_emp_api.Models
{
    public class Asset
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string AssetName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string SerialNumber { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        public bool IsAvailable { get; set; } = true;

        public bool IsDamaged { get; set; } = false;
    }
}
