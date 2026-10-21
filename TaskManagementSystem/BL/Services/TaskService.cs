using TaskManagementSystem.BL.DTOs.Tasks;
using TaskManagementSystem.DAL.Models;
using TaskManagementSystem.DAL.Repo;

namespace TaskManagementSystem.BL.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<TaskResponseDto> CreateTaskAsync(string userId, CreateTaskDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID is required.", nameof(userId));
            }

            var task = new UserTask
            {
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                TaskDate = dto.TaskDate,
                CreatedAt = DateTime.UtcNow
            };

            await _taskRepository.AddAsync(task);

            // Fetch to get user navigation loaded if needed
            var created = await _taskRepository.GetByIdAsync(task.Id) ?? task;

            return MapToResponseDto(created);
        }

        public async Task<IEnumerable<TaskResponseDto>> GetMyTasksAsync(string userId, TaskFilterDto filter)
        {
            var tasks = await _taskRepository.GetUserTasksAsync(
                userId,
                filter.Period,
                filter.FromDate,
                filter.ToDate);

            return tasks.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<TaskResponseDto>> GetAdminTasksAsync(TaskFilterDto filter)
        {
            var tasks = await _taskRepository.GetAllTasksAsync(
                filter.UserId,
                filter.Period,
                filter.FromDate,
                filter.ToDate);

            return tasks.Select(MapToResponseDto);
        }

        private static TaskResponseDto MapToResponseDto(UserTask task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                TaskDate = task.TaskDate,
                CreatedAt = task.CreatedAt,
                UserId = task.UserId,
                UserName = task.User?.FullName ?? "Unknown",
                UserEmail = task.User?.Email ?? string.Empty
            };
        }
    }
}
