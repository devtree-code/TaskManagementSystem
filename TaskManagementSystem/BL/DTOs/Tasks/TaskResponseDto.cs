namespace TaskManagementSystem.BL.DTOs.Tasks
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TaskDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }
}
