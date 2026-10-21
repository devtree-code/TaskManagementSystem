using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.DAL.Models;

namespace TaskManagementSystem.DAL.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Apply any pending migrations automatically
            await context.Database.MigrateAsync();

            // 2. Seed Roles
            string[] roles = ["Admin", "User"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 3. Seed Default Admin User
            const string adminEmail = "admin@tasksystem.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddMonths(-6)
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 4. Seed Diverse Sample Users (for testing filtering by user)
            var sampleUsers = new List<(string Email, string FullName, string Role)>
            {
                ("sarah.connor@tasksystem.com", "Sarah Connor", "User"),
                ("michael.scott@tasksystem.com", "Michael Scott", "User"),
                ("elena.rostova@tasksystem.com", "Elena Rostova", "User"),
                ("tariq.mansour@tasksystem.com", "Tariq Mansour", "User"),
                ("alex.mercer@tasksystem.com", "Alex Mercer", "User")
            };

            var userMap = new Dictionary<string, ApplicationUser>();

            foreach (var (email, fullName, role) in sampleUsers)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = fullName,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow.AddMonths(-5)
                    };

                    var createRes = await userManager.CreateAsync(user, "User@123456");
                    if (createRes.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, role);
                    }
                }
                userMap[email] = user;
            }

            // 5. Seed Diverse Tasks (spread across Today, Last Week, Last Month, Last Year, All Time)
            // Only seed if we haven't already seeded the comprehensive test dataset
            const string seedMarkerTitle = "Flutter Task Submission UI";
            bool alreadySeeded = await context.Tasks.AnyAsync(t => t.Title == seedMarkerTitle);

            if (!alreadySeeded)
            {
                var now = DateTime.UtcNow;

                var tasksToSeed = new List<UserTask>
                {
                    // ==========================================
                    // SARAH CONNOR (Mobile / Flutter Engineer)
                    // ==========================================
                    new UserTask
                    {
                        UserId = userMap["sarah.connor@tasksystem.com"].Id,
                        Title = "Flutter Task Submission UI",
                        Description = "Designed and developed the Flutter form screen allowing employees to enter task descriptions, select completed timestamps, and submit to the backend API.",
                        TaskDate = now.AddHours(-3), // Today
                        CreatedAt = now.AddHours(-2)
                    },
                    new UserTask
                    {
                        UserId = userMap["sarah.connor@tasksystem.com"].Id,
                        Title = "Secure JWT Storage in Flutter",
                        Description = "Integrated flutter_secure_storage package to store authentication tokens and automatically attach Authorization Bearer headers to HTTP interceptors.",
                        TaskDate = now.AddDays(-2), // Last Week (2 days ago)
                        CreatedAt = now.AddDays(-2).AddHours(1)
                    },
                    new UserTask
                    {
                        UserId = userMap["sarah.connor@tasksystem.com"].Id,
                        Title = "State Management with Bloc",
                        Description = "Refactored user task history view to use flutter_bloc. Handled loading, loaded, and network failure states with custom retry widgets.",
                        TaskDate = now.AddDays(-5), // Last Week (5 days ago)
                        CreatedAt = now.AddDays(-5).AddHours(2)
                    },
                    new UserTask
                    {
                        UserId = userMap["sarah.connor@tasksystem.com"].Id,
                        Title = "Offline Task Sync Service",
                        Description = "Added SQLite local database caching so employees can log tasks even while offline and batch-sync them once network connection restores.",
                        TaskDate = now.AddDays(-18), // Last Month (18 days ago)
                        CreatedAt = now.AddDays(-18).AddMinutes(45)
                    },
                    new UserTask
                    {
                        UserId = userMap["sarah.connor@tasksystem.com"].Id,
                        Title = "Android & iOS App Signing",
                        Description = "Configured keystores, provisioning profiles, and automated Fastlane pipelines for staging deployment builds.",
                        TaskDate = now.AddDays(-75), // Last Year (2.5 months ago)
                        CreatedAt = now.AddDays(-75).AddHours(3)
                    },

                    // ==========================================
                    // ELENA ROSTOVA (Senior Backend Engineer)
                    // ==========================================
                    new UserTask
                    {
                        UserId = userMap["elena.rostova@tasksystem.com"].Id,
                        Title = "Excel Export with ClosedXML",
                        Description = "Architected high-performance Excel report builder utilizing ClosedXML. Added alternating row fills, thin gray borders, title banner, and streaming download response.",
                        TaskDate = now.AddHours(-6), // Today
                        CreatedAt = now.AddHours(-5)
                    },
                    new UserTask
                    {
                        UserId = userMap["elena.rostova@tasksystem.com"].Id,
                        Title = "Dynamic Query Filtering Engine",
                        Description = "Implemented reusable repository expressions to dynamically filter task submissions by timeframe preset (week/month/year) and explicit start/end dates.",
                        TaskDate = now.AddDays(-3), // Last Week (3 days ago)
                        CreatedAt = now.AddDays(-3).AddHours(1)
                    },
                    new UserTask
                    {
                        UserId = userMap["elena.rostova@tasksystem.com"].Id,
                        Title = "SQL Index Optimization",
                        Description = "Analyzed execution plans on Tasks table. Added B-Tree composite indices on (UserId) and (TaskDate) to ensure millisecond queries over millions of records.",
                        TaskDate = now.AddDays(-12), // Last Month (12 days ago)
                        CreatedAt = now.AddDays(-12).AddMinutes(30)
                    },
                    new UserTask
                    {
                        UserId = userMap["elena.rostova@tasksystem.com"].Id,
                        Title = "Dual Authentication Pipeline",
                        Description = "Configured ASP.NET Core middleware to simultaneously support JWT Bearer tokens for mobile clients and Cookie sessions for web admin dashboard.",
                        TaskDate = now.AddDays(-25), // Last Month (25 days ago)
                        CreatedAt = now.AddDays(-25).AddHours(2)
                    },
                    new UserTask
                    {
                        UserId = userMap["elena.rostova@tasksystem.com"].Id,
                        Title = "Database Migration & Seed Script",
                        Description = "Constructed Entity Framework Core Code-First migrations and automated DB initializer with role management and admin provisioning.",
                        TaskDate = now.AddDays(-140), // Last Year (~4.5 months ago)
                        CreatedAt = now.AddDays(-140).AddHours(4)
                    },

                    // ==========================================
                    // MICHAEL SCOTT (Project Manager / Lead)
                    // ==========================================
                    new UserTask
                    {
                        UserId = userMap["michael.scott@tasksystem.com"].Id,
                        Title = "Sprint 14 Planning & Review",
                        Description = "Conducted bi-weekly sprint planning meeting with engineering team. Groomed backlog user stories, assigned story points, and resolved feature dependencies.",
                        TaskDate = now.AddDays(-1), // Yesterday
                        CreatedAt = now.AddDays(-1).AddHours(1)
                    },
                    new UserTask
                    {
                        UserId = userMap["michael.scott@tasksystem.com"].Id,
                        Title = "Client Demonstration & Walkthrough",
                        Description = "Presented interactive live demo of the Task Management Mobile App and Admin Dashboard to executive stakeholders. Gathered positive feedback on reporting speed.",
                        TaskDate = now.AddDays(-6), // Last Week (6 days ago)
                        CreatedAt = now.AddDays(-6).AddHours(2)
                    },
                    new UserTask
                    {
                        UserId = userMap["michael.scott@tasksystem.com"].Id,
                        Title = "Q4 Milestone Audit",
                        Description = "Audited project deliverables against Q4 target roadmap. Verified all planned authentication, mobile submission, and spreadsheet features are on track.",
                        TaskDate = now.AddDays(-22), // Last Month (22 days ago)
                        CreatedAt = now.AddDays(-22).AddHours(3)
                    },
                    new UserTask
                    {
                        UserId = userMap["michael.scott@tasksystem.com"].Id,
                        Title = "Security & Compliance Review",
                        Description = "Reviewed GDPR compliance requirements, password hashing standards, and data retention policies for submitted employee logs.",
                        TaskDate = now.AddDays(-210), // Last Year (7 months ago)
                        CreatedAt = now.AddDays(-210).AddHours(1)
                    },

                    // ==========================================
                    // TARIQ MANSOUR (UI/UX Specialist)
                    // ==========================================
                    new UserTask
                    {
                        UserId = userMap["tariq.mansour@tasksystem.com"].Id,
                        Title = "Admin Dashboard UX Redesign",
                        Description = "Created polished Bootstrap 5 layout featuring clean filter cards, status badges, responsive navigation bar, and high-contrast readable typography.",
                        TaskDate = now.AddHours(-10), // Today
                        CreatedAt = now.AddHours(-9)
                    },
                    new UserTask
                    {
                        UserId = userMap["tariq.mansour@tasksystem.com"].Id,
                        Title = "Mobile Dark Mode Theme",
                        Description = "Built Figma design system for Flutter mobile dark and light themes, including custom color palettes for status badges and cards.",
                        TaskDate = now.AddDays(-4), // Last Week (4 days ago)
                        CreatedAt = now.AddDays(-4).AddHours(3)
                    },
                    new UserTask
                    {
                        UserId = userMap["tariq.mansour@tasksystem.com"].Id,
                        Title = "Iconography & Accessibility Polish",
                        Description = "Audited color contrast ratios according to WCAG 2.1 AA guidelines. Updated button focus states and screen reader aria labels across admin views.",
                        TaskDate = now.AddDays(-15), // Last Month (15 days ago)
                        CreatedAt = now.AddDays(-15).AddHours(2)
                    },
                    new UserTask
                    {
                        UserId = userMap["tariq.mansour@tasksystem.com"].Id,
                        Title = "Design System Component Library",
                        Description = "Created reusable Figma UI kit with 40+ components, form inputs, date pickers, dropdowns, and data table layouts.",
                        TaskDate = now.AddDays(-90), // Last Year (3 months ago)
                        CreatedAt = now.AddDays(-90).AddHours(5)
                    },

                    // ==========================================
                    // ALEX MERCER (QA Automation Specialist)
                    // ==========================================
                    new UserTask
                    {
                        UserId = userMap["alex.mercer@tasksystem.com"].Id,
                        Title = "End-to-End API Integration Suite",
                        Description = "Wrote automated PowerShell integration test script covering registration, JWT issuance, multi-date task logging, timeframe assertions, and Excel file validation.",
                        TaskDate = now.AddHours(-1), // Today (1 hour ago)
                        CreatedAt = now.AddMinutes(-40)
                    },
                    new UserTask
                    {
                        UserId = userMap["alex.mercer@tasksystem.com"].Id,
                        Title = "Load Testing on Excel Export",
                        Description = "Simulated simultaneous export requests for large task datasets (10,000+ records) to verify memory stability and response latency under 500ms.",
                        TaskDate = now.AddDays(-2), // Last Week (2 days ago)
                        CreatedAt = now.AddDays(-2).AddHours(2)
                    },
                    new UserTask
                    {
                        UserId = userMap["alex.mercer@tasksystem.com"].Id,
                        Title = "Mobile Cross-Platform Smoke Testing",
                        Description = "Executed automated smoke test suite across 4 Android emulator devices and 2 iOS simulators to verify responsive UI rendering.",
                        TaskDate = now.AddDays(-11), // Last Month (11 days ago)
                        CreatedAt = now.AddDays(-11).AddHours(4)
                    },
                    new UserTask
                    {
                        UserId = userMap["alex.mercer@tasksystem.com"].Id,
                        Title = "Regression Test Suite v1.0",
                        Description = "Documented complete test matrix covering token refresh scenarios, expired credentials, invalid date ranges, and SQL injection protections.",
                        TaskDate = now.AddDays(-320), // Last Year (10.5 months ago)
                        CreatedAt = now.AddDays(-320).AddHours(1)
                    },
                    new UserTask
                    {
                        UserId = userMap["alex.mercer@tasksystem.com"].Id,
                        Title = "Historical Archive Legacy Test",
                        Description = "Archived legacy task records from previous fiscal year to ensure backwards compatibility with historical data imports.",
                        TaskDate = now.AddDays(-450), // All Time (>1 year ago)
                        CreatedAt = now.AddDays(-450).AddHours(2)
                    }
                };

                await context.Tasks.AddRangeAsync(tasksToSeed);
                await context.SaveChangesAsync();
            }
        }
    }
}
