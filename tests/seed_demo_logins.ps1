$b = @{ username='superadmin'; password='Admin@123456' } | ConvertTo-Json
$l = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/auth/login' -Method Post -Body $b -ContentType 'application/json'
$h = @{ Authorization = "Bearer $($l.data.accessToken)" }

# 1. Create Amina as HRManager if not exists
$hrUser = @{
    username = "amina"
    email = "amina.rahman@hrms.local"
    password = "Password@123"
    employeeId = 2
    roles = @("HRManager")
} | ConvertTo-Json

try {
    $res = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/users' -Method Post -Headers $h -Body $hrUser -ContentType 'application/json'
    Write-Host "Created HR Manager user: $($res.data.username)"
} catch {
    Write-Host "User already exists or notice: $_"
}

# 2. Test Tariqul (Employee)
$tariqulLogin = @{ username = "tariqul"; password = "Password@123" } | ConvertTo-Json
$tRes = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/auth/login' -Method Post -Body $tariqulLogin -ContentType 'application/json'
Write-Host "Tariqul Login OK! Role: $($tRes.data.user.roles -join ', '), FullName: $($tRes.data.user.fullName)"

# 3. Test Amina (HRManager)
$aminaLogin = @{ username = "amina"; password = "Password@123" } | ConvertTo-Json
$aRes = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/auth/login' -Method Post -Body $aminaLogin -ContentType 'application/json'
Write-Host "Amina Login OK! Role: $($aRes.data.user.roles -join ', '), FullName: $($aRes.data.user.fullName)"
