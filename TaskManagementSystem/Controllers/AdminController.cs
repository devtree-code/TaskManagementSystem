using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.BL.DTOs.Admin;
using TaskManagementSystem.BL.DTOs.Tasks;
using TaskManagementSystem.BL.Services;
using TaskManagementSystem.DAL.Models;

namespace TaskManagementSystem.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IAuthService _authService;
        private readonly IExcelExportService _excelExportService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AdminController(
            ITaskService taskService,
            IAuthService authService,
            IExcelExportService excelExportService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _taskService = taskService;
            _authService = authService;
            _excelExportService = excelExportService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Admin Dashboard View: Displays all submitted tasks, user dropdown, date range filters, and export button.
        /// </summary>
        [HttpGet]
        [HttpGet("index")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> Index(
            [FromQuery] string? userId = null,
            [FromQuery] string? period = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var filter = new TaskFilterDto
            {
                UserId = userId,
                Period = period,
                FromDate = fromDate,
                ToDate = toDate
            };

            var tasks = await _taskService.GetAdminTasksAsync(filter);
            var users = await _authService.GetAllUsersAsync();

            var viewModel = new AdminDashboardViewModel
            {
                Tasks = tasks,
                Users = users,
                SelectedUserId = userId,
                SelectedPeriod = period,
                FromDate = fromDate,
                ToDate = toDate
            };

            return View(viewModel);
        }

        /// <summary>
        /// Export filtered tasks to an Excel (.xlsx) file.
        /// </summary>
        [HttpGet("export")]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> Export(
            [FromQuery] string? userId = null,
            [FromQuery] string? period = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var filter = new TaskFilterDto
            {
                UserId = userId,
                Period = period,
                FromDate = fromDate,
                ToDate = toDate
            };

            var tasks = (await _taskService.GetAdminTasksAsync(filter)).ToList();

            // Build human-friendly filter summary for the Excel title banner
            var filterDescriptions = new List<string>();

            if (!string.IsNullOrWhiteSpace(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                filterDescriptions.Add($"User: {user?.FullName ?? userId}");
            }
            else
            {
                filterDescriptions.Add("User: All Users");
            }

            if (!string.IsNullOrWhiteSpace(period))
            {
                filterDescriptions.Add($"Period: {char.ToUpper(period[0]) + period.Substring(1)}");
            }

            if (fromDate.HasValue)
            {
                filterDescriptions.Add($"From: {fromDate.Value:yyyy-MM-dd}");
            }

            if (toDate.HasValue)
            {
                filterDescriptions.Add($"To: {toDate.Value:yyyy-MM-dd}");
            }

            var summary = string.Join(" | ", filterDescriptions);
            var excelBytes = _excelExportService.GenerateTasksExcel(tasks, summary);

            var fileName = $"Tasks_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(excelBytes, contentType, fileName);
        }

        /// <summary>
        /// Admin Login View (GET)
        /// </summary>
        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Index));
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new AdminLoginViewModel());
        }

        /// <summary>
        /// Process Admin Login (POST)
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                model.ErrorMessage = "Invalid email or password.";
                return View(model);
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin)
            {
                model.ErrorMessage = "Access denied. Only administrators can access this portal.";
                return View(model);
            }

            // Create Cookie Claims Principal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Admin Logout
        /// </summary>
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}
