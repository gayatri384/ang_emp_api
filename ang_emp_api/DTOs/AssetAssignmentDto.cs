namespace ang_emp_api.DTOs
{
    public class AssetAssignmentDto
    {
        public int AssignmentId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsDamagedOnReturn { get; set; }
        public string? Remarks { get; set; }
    }

}
