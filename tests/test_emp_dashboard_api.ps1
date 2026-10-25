$tLogin = @{ username='tariqul'; password='Password@123' } | ConvertTo-Json
$tRes = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/auth/login' -Method Post -Body $tLogin -ContentType 'application/json'
$h = @{ Authorization = "Bearer $($tRes.data.accessToken)" }

Write-Host "1. Testing /attendance/today..."
$att = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/attendance/today' -Headers $h -Method Get
Write-Host "Attendance Today Status: $($att.data.status), HasCheckedIn: $($att.data.hasCheckedIn)"

Write-Host "2. Testing /leave/balances..."
$lv = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/leave/balances' -Headers $h -Method Get
Write-Host "Leave Balances count: $($lv.data.Count)"
foreach ($b in $lv.data) {
    Write-Host " - $($b.leaveTypeName): Total=$($b.totalDays), Used=$($b.usedDays), Balance=$($b.balance)"
}

Write-Host "3. Testing /assets/assignments?employeeId=1..."
$ast = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/assets/assignments?employeeId=1' -Headers $h -Method Get
Write-Host "Assigned Assets count: $($ast.data.Count)"
foreach ($a in $ast.data) {
    Write-Host " - $($a.assetName) ($($a.assetCode)): Serial=$($a.serialNumber), Assigned=$($a.assignedDate)"
}

Write-Host "4. Testing /announcements..."
$ann = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/announcements' -Headers $h -Method Get
Write-Host "Announcements count: $($ann.data.Count)"

Write-Host "5. Testing /payroll/my-payslips..."
$pay = Invoke-RestMethod -Uri 'http://localhost:5219/api/v1/payroll/my-payslips' -Headers $h -Method Get
Write-Host "Payslips count: $($pay.data.Count)"
