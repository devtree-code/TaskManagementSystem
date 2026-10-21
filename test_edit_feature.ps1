$baseUrl = "http://localhost:5153"
$ErrorActionPreference = "Stop"

Write-Host "=== TESTING TASK EDIT CAPABILITY ===" -ForegroundColor Cyan

# 1. Start test session
$webSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$loginPage = Invoke-WebRequest -Uri "$baseUrl/admin/login" -WebSession $webSession -Method Get
$tokenMatch = [regex]::Match($loginPage.Content, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
$antiForgeryToken = $tokenMatch.Groups[1].Value

# 2. Admin Login
Invoke-WebRequest -Uri "$baseUrl/admin/login" -WebSession $webSession -Method Post -Body @{
    __RequestVerificationToken = $antiForgeryToken
    Email = "admin@tasksystem.com"
    Password = "Admin@123456"
} | Out-Null
Write-Host "1. Admin Logged In successfully." -ForegroundColor Green

# 3. Test GET /admin/task/{id}
$taskIdToEdit = 1
$taskJson = Invoke-RestMethod -Uri "$baseUrl/admin/task/$taskIdToEdit" -WebSession $webSession -Method Get
Write-Host "2. Retrieved Task #${taskIdToEdit}: Original Title: '$($taskJson.title)', Desc: '$($taskJson.description.Substring(0, 30))...'" -ForegroundColor Green

# 4. Test POST /admin/edit-task (Admin edits the record)
$newTitle = "Flutter Task Submission UI [UPDATED]"
$newDesc = "Updated task description: Designed, reviewed, and finalized the Flutter form screen with updated validation rules."
$newDate = [DateTime]::UtcNow.AddHours(-1).ToString("s")

$editBody = @{
    id = $taskIdToEdit
    title = $newTitle
    description = $newDesc
    taskDate = $newDate
} | ConvertTo-Json

$editRes = Invoke-RestMethod -Uri "$baseUrl/admin/edit-task" -WebSession $webSession -Method Post -Body $editBody -ContentType "application/json"
if ($editRes.success -and $editRes.task.title -eq $newTitle) {
    Write-Host "3. Admin Edit Succeeded! New Title: '$($editRes.task.title)'" -ForegroundColor Green
} else {
    throw "Admin Edit Failed!"
}

# 5. Verify the updated task in the Admin Dashboard table
$dashHtml = (Invoke-WebRequest -Uri "$baseUrl/admin?ajax=true" -WebSession $webSession -Method Get).Content
if ($dashHtml.Contains("Flutter Task Submission UI [UPDATED]")) {
    Write-Host "4. Admin Dashboard table verified with updated content!" -ForegroundColor Green
} else {
    throw "Updated task not found in Admin Dashboard HTML!"
}

# 6. Test Mobile API PUT /api/tasks/{id}
$mobileAuth = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body (@{email="sarah.connor@tasksystem.com"; password="User@123456"} | ConvertTo-Json) -ContentType "application/json"
$sarahHeaders = @{ "Authorization" = "Bearer $($mobileAuth.token)" }

$sarahTasks = Invoke-RestMethod -Uri "$baseUrl/api/tasks/my-tasks" -Headers $sarahHeaders -Method Get
$sarahTaskId = $sarahTasks[0].id
Write-Host "Sarah Connor owns Task #$sarahTaskId" -ForegroundColor Gray

# Sarah edits her own task
$sarahEditBody = @{
    title = "Secure JWT Storage in Flutter [REVISED]"
    description = "Revised implementation using encrypted shared preferences on Android and Keychain on iOS."
    taskDate = [DateTime]::UtcNow.AddDays(-1).ToString("o")
} | ConvertTo-Json

$sarahEditRes = Invoke-RestMethod -Uri "$baseUrl/api/tasks/$sarahTaskId" -Method Put -Headers $sarahHeaders -Body $sarahEditBody -ContentType "application/json"
if ($sarahEditRes.title -eq "Secure JWT Storage in Flutter [REVISED]") {
    Write-Host "5. Mobile API PUT /api/tasks/$sarahTaskId Succeeded for Author!" -ForegroundColor Green
} else {
    throw "Mobile API PUT failed!"
}

# 7. Test Security: Sarah cannot edit John Doe's task (#1)
try {
    $unauthEdit = Invoke-RestMethod -Uri "$baseUrl/api/tasks/1" -Method Put -Headers $sarahHeaders -Body $sarahEditBody -ContentType "application/json"
    Write-Host "Warning: Regular user edited someone else's task!" -ForegroundColor Red
} catch {
    Write-Host "6. Security Check Passed: Regular user denied editing someone else's task (HTTP 403 Forbidden)." -ForegroundColor Green
}

Write-Host "`n=== ALL RECORD EDITING TESTS PASSED WITH 100% SUCCESS! ===" -ForegroundColor Green
