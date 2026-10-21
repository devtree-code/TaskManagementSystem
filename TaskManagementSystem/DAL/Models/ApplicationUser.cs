using Microsoft.AspNetCore.Identity;

namespace TaskManagementSystem.DAL.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for user's tasks
        public virtual ICollection<UserTask> Tasks { get; set; } = new List<UserTask>();
    }
}
