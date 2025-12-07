namespace ang_emp_api.DTOs
{
    public class CreateTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int AssignedToId { get; set; }
        public int ColumnId { get; set; }    // REQUIRED
        public int Order { get; set; }       // REQUIRED
        public string Priority { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
