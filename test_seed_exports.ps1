$baseUrl = "http://localhost:5153"
$ErrorActionPreference = "Stop"

Write-Host "=== TESTING SEED DATA & EXCEL EXPORTS ===" -ForegroundColor Cyan

$webSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession

# 1. Admin Login
$loginPage = Invoke-WebRequest -Uri "$baseUrl/admin/login" -WebSession $webSession -Method Get
$tokenMatch = [regex]::Match($loginPage.Content, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
$antiForgeryToken = $tokenMatch.Groups[1].Value

$adminFormBody = @{
    __RequestVerificationToken = $antiForgeryToken
    Email = "admin@tasksystem.com"
    Password = "Admin@123456"
}
$adminLoginRes = Invoke-WebRequest -Uri "$baseUrl/admin/login" -WebSession $webSession -Method Post -Body $adminFormBody
Write-Host "1. Admin Logged In: Status $($adminLoginRes.StatusCode)" -ForegroundColor Green

# 2. Get Users from API to test filtering by a specific user
$users = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body (@{email="admin@tasksystem.com"; password="Admin@123456"} | ConvertTo-Json) -ContentType "application/json"
$token = $users.token

# Check Dashboard Task Count
$dashHtml = (Invoke-WebRequest -Uri "$baseUrl/admin" -WebSession $webSession -Method Get).Content
$totalMatch = [regex]::Match($dashHtml, 'Total Tasks:\s*<strong>(\d+)</strong>')
Write-Host "2. Admin Dashboard Total Tasks: $($totalMatch.Groups[1].Value)" -ForegroundColor Green

$exportDir = Join-Path $PSScriptRoot "ExportedReports"
if (-not (Test-Path $exportDir)) {
    New-Item -ItemType Directory -Path $exportDir | Out-Null
}

# 3. Export Scenario A: All Users, All Time
$fileAll = Join-Path $exportDir "Tasks_All_Users_All_Time.xlsx"
Invoke-WebRequest -Uri "$baseUrl/admin/export?period=all" -WebSession $webSession -Method Get -OutFile $fileAll
Write-Host "3. Exported [All Users - All Time]: $(Get-Item $fileAll | Select-Object -ExpandProperty Length) bytes" -ForegroundColor Green

# 4. Export Scenario B: Filtered by Last Week
$fileWeek = Join-Path $exportDir "Tasks_Last_Week.xlsx"
Invoke-WebRequest -Uri "$baseUrl/admin/export?period=week" -WebSession $webSession -Method Get -OutFile $fileWeek
Write-Host "4. Exported [Filter: Last Week]: $(Get-Item $fileWeek | Select-Object -ExpandProperty Length) bytes" -ForegroundColor Green

# 5. Export Scenario C: Filtered by Last Month
$fileMonth = Join-Path $exportDir "Tasks_Last_Month.xlsx"
Invoke-WebRequest -Uri "$baseUrl/admin/export?period=month" -WebSession $webSession -Method Get -OutFile $fileMonth
Write-Host "5. Exported [Filter: Last Month]: $(Get-Item $fileMonth | Select-Object -ExpandProperty Length) bytes" -ForegroundColor Green

# 6. Export Scenario D: Filtered by Sarah Connor (Mobile Engineer)
# Extract Sarah's User ID from the dashboard dropdown
$sarahMatch = [regex]::Match($dashHtml, '<option value="([^"]+)">Sarah Connor')
if ($sarahMatch.Success) {
    $sarahId = $sarahMatch.Groups[1].Value
    $fileSarah = Join-Path $exportDir "Tasks_Sarah_Connor_Only.xlsx"
    Invoke-WebRequest -Uri "$baseUrl/admin/export?userId=$sarahId" -WebSession $webSession -Method Get -OutFile $fileSarah
    Write-Host "6. Exported [Filter: Sarah Connor Only]: $(Get-Item $fileSarah | Select-Object -ExpandProperty Length) bytes" -ForegroundColor Green
}

# 7. Export Scenario E: Filtered by Elena Rostova (Backend Engineer)
$elenaMatch = [regex]::Match($dashHtml, '<option value="([^"]+)">Elena Rostova')
if ($elenaMatch.Success) {
    $elenaId = $elenaMatch.Groups[1].Value
    $fileElena = Join-Path $exportDir "Tasks_Elena_Rostova_Only.xlsx"
    Invoke-WebRequest -Uri "$baseUrl/admin/export?userId=$elenaId" -WebSession $webSession -Method Get -OutFile $fileElena
    Write-Host "7. Exported [Filter: Elena Rostova Only]: $(Get-Item $fileElena | Select-Object -ExpandProperty Length) bytes" -ForegroundColor Green
}

Write-Host "`n=== ALL EXPORT SCENARIOS TESTED AND GENERATED SUCCESSFULLY! ===" -ForegroundColor Green
Get-ChildItem -Path $exportDir | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
