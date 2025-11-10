namespace ang_emp_api.DTOs
{
    public class AttendanceCreateDto
    {
        // Employee will be taken from current user in usual flows; kept here for admin use
        public int? EmployeeId { get; set; }
        public DateTime Date { get; set; }        // use date-only part
        public bool IsHalfDay { get; set; } = false;
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string? Remarks { get; set; }
    }
}
