$ErrorActionPreference = 'Stop'
$loginBody = @{
    username = 'superadmin'
    password = 'Admin@123456'
} | ConvertTo-Json

$login = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/auth/login' -Method Post -Body $loginBody -ContentType 'application/json'
$token = $login.data.accessToken
Write-Host "JWT Token Acquired. User: $($login.data.user.username)"

$headers = @{
    Authorization = "Bearer $token"
}

$endpoints = @(
    '/dashboard/summary',
    '/employees?pageSize=5',
    '/attendance?pageSize=5',
    '/attendance/today',
    '/leave/applications?pageSize=5',
    '/payroll/runs?pageSize=5',
    '/recruitment/jobs?pageSize=5',
    '/recruitment/candidates?pageSize=5',
    '/recruitment/applications',
    '/performance/cycles',
    '/assets?pageSize=5',
    '/assets/categories',
    '/departments',
    '/branches',
    '/announcements',
    '/notifications',
    '/users?pageSize=5',
    '/roles',
    '/settings'
)

$allPassed = $true
foreach ($ep in $endpoints) {
    try {
        $res = Invoke-RestMethod -Uri "http://localhost:5219/api/v1$ep" -Headers $headers -Method Get
        if ($res.success) {
            Write-Host "[OK 200] $ep" -ForegroundColor Green
        } else {
            Write-Host "[FAIL] $ep - $($res.message)" -ForegroundColor Red
            $allPassed = $false
        }
    } catch {
        Write-Host "[EXCEPTION] $ep -> $($_.Exception.Message)" -ForegroundColor Red
        $allPassed = $false
    }
}

if ($allPassed) {
    Write-Host "`n>>> ALL 19 ENDPOINTS VERIFIED HEALTHY (HTTP 200 OK)! <<<" -ForegroundColor Cyan
}
