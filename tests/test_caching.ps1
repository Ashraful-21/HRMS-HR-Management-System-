$ErrorActionPreference = 'Stop'

Write-Host "Authenticating as superadmin..."
$loginBody = @{ username = 'superadmin'; password = 'Admin@123456' } | ConvertTo-Json
$loginRes = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/auth/login' -Method Post -Body $loginBody -ContentType 'application/json'
$token = $loginRes.data.accessToken
$headers = @{ Authorization = "Bearer $token" }

Write-Host "`n=== 1. Testing Branches Endpoint (/api/v1/branches/all) ==="
$sw1 = [System.Diagnostics.Stopwatch]::StartNew()
$res1 = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/branches/all' -Method Get -Headers $headers
$sw1.Stop()
Write-Host "Call 1 (DB Load -> Caches into Redis/RAM): $($sw1.ElapsedMilliseconds) ms | Branches: $($res1.data.Count)"

$sw2 = [System.Diagnostics.Stopwatch]::StartNew()
$res2 = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/branches/all' -Method Get -Headers $headers
$sw2.Stop()
Write-Host "Call 2 (Direct Cache Hit!):               $($sw2.ElapsedMilliseconds) ms | Branches: $($res2.data.Count)"

Write-Host "`n=== 2. Testing Departments Endpoint (/api/v1/departments) ==="
$sw3 = [System.Diagnostics.Stopwatch]::StartNew()
$res3 = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/departments' -Method Get -Headers $headers
$sw3.Stop()
Write-Host "Call 1 (DB Load -> Caches into Redis/RAM): $($sw3.ElapsedMilliseconds) ms | Depts: $($res3.data.Count)"

$sw4 = [System.Diagnostics.Stopwatch]::StartNew()
$res4 = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/departments' -Method Get -Headers $headers
$sw4.Stop()
Write-Host "Call 2 (Direct Cache Hit!):               $($sw4.ElapsedMilliseconds) ms | Depts: $($res4.data.Count)"

Write-Host "`n=== 3. Testing Department Tree Endpoint (/api/v1/departments/tree) ==="
$swTree1 = [System.Diagnostics.Stopwatch]::StartNew()
$resTree1 = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/departments/tree' -Method Get -Headers $headers
$swTree1.Stop()
Write-Host "Call 1 (DB Load -> Caches Tree):          $($swTree1.ElapsedMilliseconds) ms | Root Depts: $($resTree1.data.Count)"

$swTree2 = [System.Diagnostics.Stopwatch]::StartNew()
$resTree2 = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/departments/tree' -Method Get -Headers $headers
$swTree2.Stop()
Write-Host "Call 2 (Direct Tree Cache Hit!):          $($swTree2.ElapsedMilliseconds) ms | Root Depts: $($resTree2.data.Count)"

Write-Host "`n=== 4. Testing System Settings Endpoint (/api/v1/settings) ==="
$sw5 = [System.Diagnostics.Stopwatch]::StartNew()
$res5 = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/settings' -Method Get -Headers $headers
$sw5.Stop()
Write-Host "Call 1 (DB Load -> Caches Settings):      $($sw5.ElapsedMilliseconds) ms | Settings: $($res5.data.Count)"

$sw6 = [System.Diagnostics.Stopwatch]::StartNew()
$res6 = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/settings' -Method Get -Headers $headers
$sw6.Stop()
Write-Host "Call 2 (Direct Cache Hit!):               $($sw6.ElapsedMilliseconds) ms | Settings: $($res6.data.Count)"

Write-Host "`n=== 5. Testing Cache Invalidation on Setting Update ==="
$currentName = ($res5.data | Where-Object { $_.key -eq 'Company.Name' }).value
$newName = if ($currentName -eq 'HRMS Global Corp') { 'HRMS Enterprise Solutions' } else { 'HRMS Global Corp' }
$updateBody = @{ key = 'Company.Name'; value = $newName } | ConvertTo-Json
$updateRes = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/settings' -Method Put -Headers $headers -Body $updateBody -ContentType 'application/json'
Write-Host "Setting updated: $($updateRes.data.key) => $($updateRes.data.value)"

$sw7 = [System.Diagnostics.Stopwatch]::StartNew()
$res7 = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/settings' -Method Get -Headers $headers
$sw7.Stop()
Write-Host "Call after Invalidation (DB Reload):      $($sw7.ElapsedMilliseconds) ms | New Value: $(($res7.data | Where-Object { $_.key -eq 'Company.Name' }).value)"

$sw8 = [System.Diagnostics.Stopwatch]::StartNew()
$res8 = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/settings' -Method Get -Headers $headers
$sw8.Stop()
Write-Host "Call after Re-cache (Direct Cache Hit!):  $($sw8.ElapsedMilliseconds) ms | New Value: $(($res8.data | Where-Object { $_.key -eq 'Company.Name' }).value)"
