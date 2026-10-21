using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.DAL.Data;
using TaskManagementSystem.DAL.Models;

namespace TaskManagementSystem.DAL.Repo
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserTask> AddAsync(UserTask task)
        {
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<UserTask?> GetByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<UserTask>> GetUserTasksAsync(string userId, string? period = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Tasks
                .Include(t => t.User)
                .Where(t => t.UserId == userId)
                .AsQueryable();

            query = ApplyFilters(query, period, fromDate, toDate);

            return await query
                .OrderByDescending(t => t.TaskDate)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserTask>> GetAllTasksAsync(string? userId = null, string? period = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Tasks
                .Include(t => t.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(userId))
            {
                query = query.Where(t => t.UserId == userId);
            }

            query = ApplyFilters(query, period, fromDate, toDate);

            return await query
                .OrderByDescending(t => t.TaskDate)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        private static IQueryable<UserTask> ApplyFilters(IQueryable<UserTask> query, string? period, DateTime? fromDate, DateTime? toDate)
        {
            var now = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(period))
            {
                switch (period.Trim().ToLowerInvariant())
                {
                    case "week":
                        var weekAgo = now.AddDays(-7);
                        query = query.Where(t => t.TaskDate >= weekAgo);
                        break;
                    case "month":
                        var monthAgo = now.AddMonths(-1);
                        query = query.Where(t => t.TaskDate >= monthAgo);
                        break;
                    case "year":
                        var yearAgo = now.AddYears(-1);
                        query = query.Where(t => t.TaskDate >= yearAgo);
                        break;
                    case "all":
                        // No timeframe restriction
                        break;
                }
            }

            if (fromDate.HasValue)
            {
                query = query.Where(t => t.TaskDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // Ensure date encompasses through the end of the specified day if time is midnight
                var inclusiveToDate = toDate.Value.TimeOfDay == TimeSpan.Zero
                    ? toDate.Value.Date.AddDays(1).AddTicks(-1)
                    : toDate.Value;
                query = query.Where(t => t.TaskDate <= inclusiveToDate);
            }

            return query;
        }
    }
}
