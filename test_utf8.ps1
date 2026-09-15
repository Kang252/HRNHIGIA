$loginPage = Invoke-WebRequest -Uri "http://localhost:5116/Account/Login" -SessionVariable session -UseBasicParsing
$token = ""
if ($loginPage.Content -match 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"') {
    $token = $matches[1]
}

$loginBody = @{
    "__RequestVerificationToken" = $token
    "Username" = "admin"
    "Password" = "123456"
}

$loginResp = Invoke-WebRequest -Uri "http://localhost:5116/Account/Login" -Method Post -Body $loginBody -WebSession $session -UseBasicParsing
$payrollResp = Invoke-WebRequest -Uri "http://localhost:5116/Work?kind=payroll&pyPeriod=2026-02" -WebSession $session -UseBasicParsing

# Save bytes to file to check encoding
[System.IO.File]::WriteAllBytes("C:\HRNHIGIA-main\HRNHIGIA-main\resp.html", $payrollResp.RawContentStream.ToArray())

$bytes = [System.IO.File]::ReadAllBytes("C:\HRNHIGIA-main\HRNHIGIA-main\resp.html")
$utf8Text = [System.Text.Encoding]::UTF8.GetString($bytes)

Write-Host "Contains 'Slide 6':" $utf8Text.Contains("Slide 6")
Write-Host "Contains 'Slide 7':" $utf8Text.Contains("Slide 7")
Write-Host "Contains 'modalBatchApprove':" $utf8Text.Contains("modalBatchApprove")
Write-Host "Contains 'modalBatchReject':" $utf8Text.Contains("modalBatchReject")
Write-Host "Contains 'modalViewPayslip':" $utf8Text.Contains("modalViewPayslip")
Write-Host "Contains 'data-toggle=""modal""':" $utf8Text.Contains('data-toggle="modal"')
Write-Host "Contains '1,458,951,350':" $utf8Text.Contains("1,458,951,350")
Write-Host "Contains '195,163,500':" $utf8Text.Contains("195,163,500")
Write-Host "Contains '122,356,381':" $utf8Text.Contains("122,356,381")
Write-Host "Contains '187':" $utf8Text.Contains("187")
Write-Host "Contains 'btn-payroll-approve':" $utf8Text.Contains("btn-payroll-approve")
Write-Host "Contains 'btn-payroll-reject':" $utf8Text.Contains("btn-payroll-reject")
Write-Host "Contains 'viewPayslipDetail':" $utf8Text.Contains("viewPayslipDetail")

# Check Vietnamese text decoded as UTF-8
$utf8QuyTrinh = [System.Text.Encoding]::UTF8.GetString([System.Text.Encoding]::UTF8.GetBytes("Quy trình phê duyệt bảng lương"))
Write-Host "Contains Vietnamese 'Quy trình phê duyệt bảng lương':" $utf8Text.Contains($utf8QuyTrinh)

