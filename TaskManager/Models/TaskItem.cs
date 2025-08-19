using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace TaskManager.Models
{
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        public string?  Description { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, InProgress, Done, Blocked

        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical

        public DateTime? DueDate { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        [JsonIgnore]
        public ICollection<TaskAssignment>? TaskAssignments { get; set; }
        [JsonIgnore]
        public ICollection<TaskDependency>? Dependencies { get; set; } // Tasks this depends on
        [JsonIgnore]
        public ICollection<TaskDependency>? DependedOnBy { get; set; } // Tasks depending on this
    }
}
