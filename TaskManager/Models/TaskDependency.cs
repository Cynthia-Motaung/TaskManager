namespace TaskManager.Models
{
    public class TaskDependency
    {
        public int TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; }

        public int DependsOnTaskId { get; set; }
        public TaskItem DependsOnTask { get; set; }
    }
}
