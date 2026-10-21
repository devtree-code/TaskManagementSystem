using TaskManagementSystem.BL.DTOs.Tasks;

namespace TaskManagementSystem.BL.Services
{
    public interface ITaskService
    {
        Task<TaskResponseDto> CreateTaskAsync(string userId, CreateTaskDto dto);
        Task<TaskResponseDto?> GetTaskByIdAsync(int id);
        Task<IEnumerable<TaskResponseDto>> GetMyTasksAsync(string userId, TaskFilterDto filter);
        Task<IEnumerable<TaskResponseDto>> GetAdminTasksAsync(TaskFilterDto filter);
        Task<TaskResponseDto> UpdateTaskAsync(int id, string currentUserId, bool isAdmin, UpdateTaskDto dto);
    }
}
