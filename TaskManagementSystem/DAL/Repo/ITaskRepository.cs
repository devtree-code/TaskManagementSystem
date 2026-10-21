using TaskManagementSystem.DAL.Models;

namespace TaskManagementSystem.DAL.Repo
{
    public interface ITaskRepository
    {
        Task<UserTask> AddAsync(UserTask task);
        Task<UserTask?> GetByIdAsync(int id);
        Task<IEnumerable<UserTask>> GetUserTasksAsync(string userId, string? period = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<UserTask>> GetAllTasksAsync(string? userId = null, string? period = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task SaveChangesAsync();
    }
}
