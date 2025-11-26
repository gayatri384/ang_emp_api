namespace ang_emp_api.Models
{
    public class WorkTask
    {
        public int Id { get; set; }

        // Work information
        public string Title { get; set; }
        public string Description { get; set; }

        // For Kanban
        public int ColumnId { get; set; }
        public KanbanColumn Column { get; set; }
        public int Order { get; set; }

        // Employee assignment
        public int AssignedToId { get; set; }      
        public Employee AssignedTo { get; set; }

        // Tracking
        public DateTime AssignedAt { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; } // Low/Medium/High
    }
}
