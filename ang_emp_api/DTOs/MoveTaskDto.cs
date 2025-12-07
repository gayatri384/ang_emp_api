namespace ang_emp_api.DTOs
{
    public class MoveTaskDto
    {
        public int TaskId { get; set; }
        public int FromColumnId { get; set; }
        public int ToColumnId { get; set; }
        public int NewOrder { get; set; }
    }
}
