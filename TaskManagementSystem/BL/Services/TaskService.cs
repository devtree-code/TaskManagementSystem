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

        public async Task<TaskResponseDto?> GetTaskByIdAsync(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            return task == null ? null : MapToResponseDto(task);
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

        public async Task<TaskResponseDto> UpdateTaskAsync(int id, string currentUserId, bool isAdmin, UpdateTaskDto dto)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
            {
                throw new KeyNotFoundException($"Task with ID #{id} was not found.");
            }

            // Authorization check: Admin can edit any task; regular users can only edit their own task
            if (!isAdmin && task.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException("You are not authorized to edit this task.");
            }

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.TaskDate = dto.TaskDate;

            await _taskRepository.UpdateAsync(task);

            var updated = await _taskRepository.GetByIdAsync(id) ?? task;
            return MapToResponseDto(updated);
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
