namespace ang_emp_api.DTOs
{
    public class MarkAttendanceDto
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } // "Present", "Absent", "Leave"
    }
}
