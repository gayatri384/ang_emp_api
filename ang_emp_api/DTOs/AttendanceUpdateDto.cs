namespace ang_emp_api.DTOs
{
    public class AttendanceUpdateDto
    {
        public int Id { get; set; }
        public int? EmployeeId { get; set; }      // optional change target
        public DateTime Date { get; set; }
        public int Status { get; set; }           // map to AttendanceStatus
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string? Remarks { get; set; }
    }
}
