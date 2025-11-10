namespace ang_emp_api.DTOs
{
    public class CreateAssetDto
    {
        public string AssetName { get; set; } = string.Empty;
        public string? AssetType { get; set; }
        public string? SerialNumber { get; set; }
        public string? Description { get; set; }
    }
}
