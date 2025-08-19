namespace TaskManager.Models
{
    public class TaskAssignment
    {

        public int UserId { get; set; }
        public User? User { get; set; }

        public int TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }
    }
}
