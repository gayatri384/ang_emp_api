namespace ang_emp_api.DTOs
{
    public class AssetDto
    {
        public int Id { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public bool IsAvailable { get; set; }
        public bool IsDamaged { get; set; }
    }
}
