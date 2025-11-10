namespace ang_emp_api.DTOs
{
    public class LeaveUpdateDto
    {
        public int LeaveId { get; set; }
        public string Status { get; set; }   // Approved / Rejected / Cancelled
    }
}
