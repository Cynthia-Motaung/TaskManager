
using System.ComponentModel.DataAnnotations;


namespace TaskManager.Models
{
    public class Attachment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string FileUrl { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
