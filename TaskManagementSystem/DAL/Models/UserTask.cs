using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementSystem.DAL.Models
{
    public class UserTask
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The date and time when the user performed the task.
        /// </summary>
        [Required]
        public DateTime TaskDate { get; set; }

        /// <summary>
        /// The date and time when the task was submitted to the system.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }
    }
}
