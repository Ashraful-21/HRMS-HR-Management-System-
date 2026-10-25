$b = @{ username='superadmin'; password='Admin@123456' } | ConvertTo-Json
$l = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/auth/login' -Method Post -Body $b -ContentType 'application/json'
$h = @{ Authorization = "Bearer $($l.data.accessToken)" }

$roles = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/roles' -Headers $h -Method Get
foreach ($r in $roles.data) {
    Write-Host "Role: $($r.name) - ID: $($r.id)"
}

# Login as Tariqul (Employee) to see what user claims / permissions are returned:
$tLogin = @{ username='tariqul'; password='Password@123' } | ConvertTo-Json
$tRes = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/auth/login' -Method Post -Body $tLogin -ContentType 'application/json'
Write-Host "`nTariqul (Employee) Roles:" ($tRes.data.user.roles -join ', ')
Write-Host "Tariqul (Employee) Permissions:" ($tRes.data.user.permissions -join ', ')
