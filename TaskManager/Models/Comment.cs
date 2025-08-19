using System;
using System.ComponentModel.DataAnnotations;
using TaskManager.Models;

namespace TaskManager.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
