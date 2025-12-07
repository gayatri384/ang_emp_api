namespace ang_emp_api.DTOs
{
    public class UpdateTaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int AssignedToId { get; set; }
        public int ColumnId { get; set; }
        public int Order { get; set; }   
        public string? Priority { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
