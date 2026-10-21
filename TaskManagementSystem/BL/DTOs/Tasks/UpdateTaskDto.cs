using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.BL.DTOs.Tasks
{
    public class UpdateTaskDto
    {
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Task description is required.")]
        [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters.")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// When the user did / completed the task.
        /// </summary>
        [Required(ErrorMessage = "Task date (when the task was performed) is required.")]
        public DateTime TaskDate { get; set; }
    }
}
