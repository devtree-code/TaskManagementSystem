using TaskManagementSystem.BL.DTOs.Tasks;

namespace TaskManagementSystem.BL.Services
{
    public interface IExcelExportService
    {
        byte[] GenerateTasksExcel(IEnumerable<TaskResponseDto> tasks, string? filterSummary = null);
    }
}
