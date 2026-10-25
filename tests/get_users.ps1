$b = @{ username='superadmin'; password='Admin@123456' } | ConvertTo-Json
$l = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/auth/login' -Method Post -Body $b -ContentType 'application/json'
$h = @{ Authorization = "Bearer $($l.data.accessToken)" }

Write-Host "--- USERS ---"
$u = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/users' -Headers $h -Method Get
$u.data.items | Format-Table id, username, email, employeeId, employeeCode, employeeName, isActive, roles

Write-Host "--- EMPLOYEES ---"
$e = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/employees?pageSize=10' -Headers $h -Method Get
$e.data.items | Format-Table id, employeeCode, fullName, workEmail, departmentName, designationTitle, status

Write-Host "--- ROLES ---"
$r = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/roles' -Headers $h -Method Get
$r.data | Format-Table id, name, description
