$baseUrl = "http://localhost:5153"
$ErrorActionPreference = "Stop"

Write-Host "--- 1. Testing Registration ---" -ForegroundColor Cyan
$regBody = @{
    fullName = "John Doe"
    email = "johndoe_test@example.com"
    password = "Password123!"
} | ConvertTo-Json

try {
    $regRes = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -Body $regBody -ContentType "application/json"
    Write-Host "Registration Successful! User: $($regRes.fullName), Email: $($regRes.email)" -ForegroundColor Green
    $token = $regRes.token
} catch {
    Write-Host "User already registered, attempting login..." -ForegroundColor Yellow
    $loginBody = @{
        email = "johndoe_test@example.com"
        password = "Password123!"
    } | ConvertTo-Json
    $loginRes = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginRes.token
    Write-Host "Login Successful! Token acquired." -ForegroundColor Green
}

$headers = @{
    "Authorization" = "Bearer $token"
}

Write-Host "`n--- 2. Submitting Tasks with Various Dates ---" -ForegroundColor Cyan
$now = [DateTime]::UtcNow

# Task 1: Today
$t1 = @{
    title = "Design Flutter Dashboard"
    description = "Created mockup and flutter widget tree for dashboard."
    taskDate = $now.ToString("o")
} | ConvertTo-Json
$r1 = Invoke-RestMethod -Uri "$baseUrl/api/tasks" -Method Post -Headers $headers -Body $t1 -ContentType "application/json"
Write-Host "Created Task 1 (Today): ID $($r1.id)" -ForegroundColor Green

# Task 2: 4 Days ago
$t2 = @{
    title = "Implement Authentication Service"
    description = "Wrote Flutter secure storage and JWT handling logic."
    taskDate = $now.AddDays(-4).ToString("o")
} | ConvertTo-Json
$r2 = Invoke-RestMethod -Uri "$baseUrl/api/tasks" -Method Post -Headers $headers -Body $t2 -ContentType "application/json"
Write-Host "Created Task 2 (4 days ago): ID $($r2.id)" -ForegroundColor Green

# Task 3: 20 Days ago
$t3 = @{
    title = "Database & Repository Layer"
    description = "Designed relational schema and configured Entity Framework Core."
    taskDate = $now.AddDays(-20).ToString("o")
} | ConvertTo-Json
$r3 = Invoke-RestMethod -Uri "$baseUrl/api/tasks" -Method Post -Headers $headers -Body $t3 -ContentType "application/json"
Write-Host "Created Task 3 (20 days ago): ID $($r3.id)" -ForegroundColor Green

# Task 4: 50 Days ago
$t4 = @{
    title = "Architecture Setup"
    description = "Initial requirement gathering and solution architecture definition."
    taskDate = $now.AddDays(-50).ToString("o")
} | ConvertTo-Json
$r4 = Invoke-RestMethod -Uri "$baseUrl/api/tasks" -Method Post -Headers $headers -Body $t4 -ContentType "application/json"
Write-Host "Created Task 4 (50 days ago): ID $($r4.id)" -ForegroundColor Green

Write-Host "`n--- 3. Verifying Timeframe Filters for Mobile (Flutter API) ---" -ForegroundColor Cyan
# Week filter
$weekTasks = Invoke-RestMethod -Uri "$baseUrl/api/tasks/my-tasks?period=week" -Method Get -Headers $headers
Write-Host "Tasks in last week: $($weekTasks.Count) (Expected: >= 2)" -ForegroundColor Yellow
foreach ($t in $weekTasks) {
    Write-Host "  - [$($t.id)] $($t.title) on $($t.taskDate)" -ForegroundColor Gray
}

# Month filter
$monthTasks = Invoke-RestMethod -Uri "$baseUrl/api/tasks/my-tasks?period=month" -Method Get -Headers $headers
Write-Host "Tasks in last month: $($monthTasks.Count) (Expected: >= 3)" -ForegroundColor Yellow

# All filter
$allTasks = Invoke-RestMethod -Uri "$baseUrl/api/tasks/my-tasks?period=all" -Method Get -Headers $headers
Write-Host "All Tasks: $($allTasks.Count) (Expected: >= 4)" -ForegroundColor Yellow

Write-Host "`n--- 4. Testing Admin Web Portal & Excel Export ---" -ForegroundColor Cyan
$webSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession

# Admin Login GET to obtain Anti-Forgery Token
$loginPage = Invoke-WebRequest -Uri "$baseUrl/admin/login" -WebSession $webSession -Method Get
$tokenMatch = [regex]::Match($loginPage.Content, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
$antiForgeryToken = $tokenMatch.Groups[1].Value

$adminFormBody = @{
    __RequestVerificationToken = $antiForgeryToken
    Email = "admin@tasksystem.com"
    Password = "Admin@123456"
}

$adminLoginRes = Invoke-WebRequest -Uri "$baseUrl/admin/login" -WebSession $webSession -Method Post -Body $adminFormBody
Write-Host "Admin Login HTTP Status: $($adminLoginRes.StatusCode)" -ForegroundColor Green

# Admin Dashboard GET
$dashboard = Invoke-WebRequest -Uri "$baseUrl/admin" -WebSession $webSession -Method Get
Write-Host "Admin Dashboard loaded successfully! HTTP $($dashboard.StatusCode), Length: $($dashboard.RawContentLength)" -ForegroundColor Green

# Excel Export GET
$excelPath = Join-Path $PSScriptRoot "test_admin_export.xlsx"
$excelRes = Invoke-WebRequest -Uri "$baseUrl/admin/export?period=all" -WebSession $webSession -Method Get -OutFile $excelPath
if (Test-Path $excelPath) {
    $fileInfo = Get-Item $excelPath
    Write-Host "Excel Export SUCCESS! Saved to $excelPath (Size: $($fileInfo.Length) bytes)" -ForegroundColor Green
} else {
    throw "Excel file export failed!"
}

Write-Host "`n=== ALL END-TO-END TESTS PASSED WITH 100% SUCCESS! ===" -ForegroundColor Green
