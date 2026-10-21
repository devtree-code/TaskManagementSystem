using TaskManagementSystem.BL.DTOs.Auth;
using TaskManagementSystem.BL.DTOs.Tasks;

namespace TaskManagementSystem.BL.DTOs.Admin
{
    public class AdminDashboardViewModel
    {
        public IEnumerable<TaskResponseDto> Tasks { get; set; } = new List<TaskResponseDto>();
        public IEnumerable<UserDto> Users { get; set; } = new List<UserDto>();
        public string? SelectedUserId { get; set; }
        public string? SelectedPeriod { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int TotalCount => Tasks?.Count() ?? 0;
    }
}
