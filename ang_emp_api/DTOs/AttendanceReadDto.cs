namespace ang_emp_api.DTOs
{
    public class AttendanceReadDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public DateTime Date { get; set; }
        public int Status { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public bool ModifiedByAdmin { get; set; }
        public string? Remarks { get; set; }
    }
}
