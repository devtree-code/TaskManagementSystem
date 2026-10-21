using ClosedXML.Excel;
using TaskManagementSystem.BL.DTOs.Tasks;

namespace TaskManagementSystem.BL.Services
{
    public class ExcelExportService : IExcelExportService
    {
        public byte[] GenerateTasksExcel(IEnumerable<TaskResponseDto> tasks, string? filterSummary = null)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Tasks Report");

            // Enable grid lines
            worksheet.ShowGridLines = true;

            int currentRow = 1;

            // Optional title banner
            worksheet.Cell(currentRow, 1).Value = "Task Management System — Export Report";
            worksheet.Range(currentRow, 1, currentRow, 6).Merge();
            worksheet.Row(currentRow).Height = 28;
            var titleRange = worksheet.Range(currentRow, 1, currentRow, 6);
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 14;
            titleRange.Style.Font.FontColor = XLColor.White;
            titleRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0F2C59"); // Deep Navy
            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            currentRow++;

            // Report metadata row
            var subtitle = string.IsNullOrWhiteSpace(filterSummary)
                ? $"Exported on: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC | Total Tasks: {tasks.Count()}"
                : $"Filters: {filterSummary} | Exported: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC | Total: {tasks.Count()}";

            worksheet.Cell(currentRow, 1).Value = subtitle;
            worksheet.Range(currentRow, 1, currentRow, 6).Merge();
            worksheet.Row(currentRow).Height = 20;
            var subtitleRange = worksheet.Range(currentRow, 1, currentRow, 6);
            subtitleRange.Style.Font.Italic = true;
            subtitleRange.Style.Font.FontSize = 10;
            subtitleRange.Style.Font.FontColor = XLColor.FromHtml("#495057");
            subtitleRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#F8F9FA");
            subtitleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            subtitleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            currentRow++;

            // Table Headers
            var headers = new[]
            {
                "Task #",
                "Employee Name",
                "Email",
                "When Did They Do It (Task Date)",
                "Submission Date (Created At)",
                "Task Description"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var colIndex = i + 1;
                var cell = worksheet.Cell(currentRow, colIndex);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 11;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E79");
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Alignment.Horizontal = i == 0 ? XLAlignmentHorizontalValues.Center : XLAlignmentHorizontalValues.Left;
            }
            worksheet.Row(currentRow).Height = 24;

            int headerRow = currentRow;
            currentRow++;

            // Populate Data Rows
            var taskList = tasks.ToList();
            for (int i = 0; i < taskList.Count; i++)
            {
                var item = taskList[i];
                var row = worksheet.Row(currentRow);

                // Alternating zebra striping
                var rowBgColor = (i % 2 == 0) ? XLColor.White : XLColor.FromHtml("#F2F4F7");

                // Col 1: ID
                var c1 = worksheet.Cell(currentRow, 1);
                c1.Value = item.Id;
                c1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Col 2: User Name
                var c2 = worksheet.Cell(currentRow, 2);
                c2.Value = item.UserName;

                // Col 3: Email
                var c3 = worksheet.Cell(currentRow, 3);
                c3.Value = item.UserEmail;

                // Col 4: Task Date (When Did They Do It)
                var c4 = worksheet.Cell(currentRow, 4);
                c4.Value = item.TaskDate.ToString("yyyy-MM-dd HH:mm");
                c4.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Col 5: Created At (When Submitted)
                var c5 = worksheet.Cell(currentRow, 5);
                c5.Value = item.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                c5.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Col 6: Description
                var c6 = worksheet.Cell(currentRow, 6);
                c6.Value = string.IsNullOrWhiteSpace(item.Title)
                    ? item.Description
                    : $"[{item.Title}] {item.Description}";
                c6.Style.Alignment.WrapText = true;

                // Apply styling across this data row
                var dataRowRange = worksheet.Range(currentRow, 1, currentRow, 6);
                dataRowRange.Style.Fill.BackgroundColor = rowBgColor;
                dataRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#D3D3D3");
                dataRowRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#E0E0E0");
                dataRowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                row.Height = 22;
                currentRow++;
            }

            // If empty dataset, display notice row
            if (taskList.Count == 0)
            {
                worksheet.Cell(currentRow, 1).Value = "No tasks found matching the selected filter criteria.";
                worksheet.Range(currentRow, 1, currentRow, 6).Merge();
                var emptyRange = worksheet.Range(currentRow, 1, currentRow, 6);
                emptyRange.Style.Font.Italic = true;
                emptyRange.Style.Font.FontColor = XLColor.Gray;
                emptyRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                emptyRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Row(currentRow).Height = 24;
            }

            // Adjust Column Widths
            worksheet.Column(1).Width = 10; // Task #
            worksheet.Column(2).Width = 24; // Name
            worksheet.Column(3).Width = 28; // Email
            worksheet.Column(4).Width = 28; // Task Date
            worksheet.Column(5).Width = 28; // Submission Date
            worksheet.Column(6).Width = 50; // Description

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
