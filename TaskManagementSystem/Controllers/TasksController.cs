using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.BL.DTOs.Tasks;
using TaskManagementSystem.BL.Services;

namespace TaskManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>
        /// Submit a new task completed by the authenticated user.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "User identifier could not be determined from token." });
            }

            try
            {
                var result = await _taskService.CreateTaskAsync(userId, dto);
                return CreatedAtAction(nameof(GetMyTasks), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieve submitted tasks for the logged-in user, filtered and ordered by timeframe (week, month, year, or date range).
        /// </summary>
        /// <param name="period">Preset timeframe: 'week', 'month', 'year', or 'all'.</param>
        /// <param name="fromDate">Optional start date filter.</param>
        /// <param name="toDate">Optional end date filter.</param>
        [HttpGet("my-tasks")]
        [ProducesResponseType(typeof(IEnumerable<TaskResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyTasks(
            [FromQuery] string? period = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "User identifier could not be determined from token." });
            }

            var filter = new TaskFilterDto
            {
                UserId = userId,
                Period = period,
                FromDate = fromDate,
                ToDate = toDate
            };

            var tasks = await _taskService.GetMyTasksAsync(userId, filter);
            return Ok(tasks);
        }
    }
}
