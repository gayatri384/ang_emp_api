namespace ang_emp_api.DTOs
{
    public class AssignAssetRequestDto
    {
        public int EmployeeId { get; set; }
        public int AssetId { get; set; }
        public string? Remarks { get; set; }
        public bool IsDamagedOnReturn { get; set; }
    }
}
