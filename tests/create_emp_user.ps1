$b = @{ username='superadmin'; password='Admin@123456' } | ConvertTo-Json
$l = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/auth/login' -Method Post -Body $b -ContentType 'application/json'
$h = @{ Authorization = "Bearer $($l.data.accessToken)" }

$newUser = @{
    username = "tariqul"
    email = "tariqul.islam@hrms.local"
    password = "Password@123"
    employeeId = 1
    roles = @("Employee")
} | ConvertTo-Json

try {
    $res = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/users' -Method Post -Headers $h -Body $newUser -ContentType 'application/json'
    Write-Host "Created user: $($res.data.username) linked to Employee ID: $($res.data.employeeId)"
} catch {
    Write-Host "Creation info: $_"
}

# Test logging in as the employee with username!
$empLogin = @{
    username = "tariqul"
    password = "Password@123"
} | ConvertTo-Json

$empRes = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/auth/login' -Method Post -Body $empLogin -ContentType 'application/json'
Write-Host "Employee Login with Username Successful! User: $($empRes.data.user.fullName), Code: $($empRes.data.user.employeeCode), Roles: $($empRes.data.user.roles -join ', ')"

# Test logging in as the employee with work email!
$emailLogin = @{
    username = "tariqul.islam@hrms.local"
    password = "Password@123"
} | ConvertTo-Json

$emailRes = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/auth/login' -Method Post -Body $emailLogin -ContentType 'application/json'
Write-Host "Employee Login with Work Email Successful! User: $($emailRes.data.user.fullName), Code: $($emailRes.data.user.employeeCode), Roles: $($emailRes.data.user.roles -join ', ')"

