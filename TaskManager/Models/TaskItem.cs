using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, InProgress, Done, Blocked

        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical

        public DateTime? DueDate { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public ICollection<TaskAssignment> TaskAssignments { get; set; }
        public ICollection<TaskDependency> Dependencies { get; set; } // Tasks this depends on
        public ICollection<TaskDependency> DependedOnBy { get; set; } // Tasks depending on this
    }
}
