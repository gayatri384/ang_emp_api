namespace ang_emp_api.DTOs
{
    public class AttendanceResponseDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } // optional
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }
}
