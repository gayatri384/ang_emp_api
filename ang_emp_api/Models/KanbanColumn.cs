using System.ComponentModel.DataAnnotations;

namespace ang_emp_api.Models
{
    public class KanbanColumn
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }  // To Do, In Progress, Review, Completed
        public int Order { get; set; }
    }
}
