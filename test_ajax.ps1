$s = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$p = Invoke-WebRequest -Uri "http://localhost:5153/admin/login" -WebSession $s
$m = [regex]::Match($p.Content, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
$t = $m.Groups[1].Value

# Login
Invoke-WebRequest -Uri "http://localhost:5153/admin/login" -WebSession $s -Method Post -Body @{
    __RequestVerificationToken = $t
    Email = "admin@tasksystem.com"
    Password = "Admin@123456"
} | Out-Null

# Test AJAX request for Last Week filter
$ajax = Invoke-WebRequest -Uri "http://localhost:5153/admin?period=week" -WebSession $s -Headers @{"X-Requested-With"="XMLHttpRequest"}

Write-Host "AJAX Request Succeeded! HTTP $($ajax.StatusCode)" -ForegroundColor Green
Write-Host "Response length: $($ajax.RawContentLength)" -ForegroundColor Green
$hasPartialInput = $ajax.Content.Contains('id="partialTotalCount"')
Write-Host "Contains partialTotalCount: $hasPartialInput" -ForegroundColor Green
$hasTable = $ajax.Content.Contains('id="tasksTable"')
Write-Host "Contains tasksTable: $hasTable" -ForegroundColor Green
$isFullHtml = $ajax.Content.Contains('<!DOCTYPE html>')
Write-Host "Is full layout excluded (clean partial): $(-not $isFullHtml)" -ForegroundColor Green
