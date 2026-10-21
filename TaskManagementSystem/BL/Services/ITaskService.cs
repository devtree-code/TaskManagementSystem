using TaskManagementSystem.BL.DTOs.Tasks;

namespace TaskManagementSystem.BL.Services
{
    public interface ITaskService
    {
        Task<TaskResponseDto> CreateTaskAsync(string userId, CreateTaskDto dto);
        Task<IEnumerable<TaskResponseDto>> GetMyTasksAsync(string userId, TaskFilterDto filter);
        Task<IEnumerable<TaskResponseDto>> GetAdminTasksAsync(TaskFilterDto filter);
    }
}
