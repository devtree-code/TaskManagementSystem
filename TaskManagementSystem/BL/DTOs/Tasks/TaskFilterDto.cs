namespace TaskManagementSystem.BL.DTOs.Tasks
{
    public class TaskFilterDto
    {
        public string? UserId { get; set; }
        public string? Period { get; set; } // "week", "month", "year", "all"
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
