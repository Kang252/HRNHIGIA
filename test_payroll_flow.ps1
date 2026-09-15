$loginPage = Invoke-WebRequest -Uri "http://localhost:5116/Account/Login" -SessionVariable session -UseBasicParsing
$token = ""
if ($loginPage.Content -match 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"') {
    $token = $matches[1]
}

Write-Host "Anti-forgery token extracted: $(if($token){'Yes'}else{'No'})"

$loginBody = @{
    "__RequestVerificationToken" = $token
    "Username" = "admin"
    "Password" = "123456"
}

$loginResp = Invoke-WebRequest -Uri "http://localhost:5116/Account/Login" -Method Post -Body $loginBody -WebSession $session -UseBasicParsing
Write-Host "Login response status: $($loginResp.StatusCode)"

$payrollResp = Invoke-WebRequest -Uri "http://localhost:5116/Work?kind=payroll&pyPeriod=2026-02" -WebSession $session -UseBasicParsing

$html = $payrollResp.Content
$hasWorkflow = $html.Contains("Quy trình phê duyệt bảng lương")
$hasApproveBtn = $html.Contains("Duyệt bảng lương")
$hasModalApprove = $html.Contains("modalBatchApprove")
$hasModalReject = $html.Contains("modalBatchReject")
$hasModalPayslip = $html.Contains("modalViewPayslip")
$hasDataToggle = $html.Contains('data-toggle="modal"')
$hasKpiTotal = $html.Contains("1,458,951,350")
$hasMojibake = $html.Contains("Phiáº¿u") -or $html.Contains("PhÃª")

Write-Host "--- TEST RESULTS ---"
Write-Host "Has Workflow (Slide 6): $hasWorkflow"
Write-Host "Has Approve Button: $hasApproveBtn"
Write-Host "Has Modal Approve: $hasModalApprove"
Write-Host "Has Modal Reject: $hasModalReject"
Write-Host "Has Modal Payslip: $hasModalPayslip"
Write-Host "Has Bootstrap 3 data-toggle: $hasDataToggle"
Write-Host "Has Total Net Salary (Slide 7): $hasKpiTotal"
Write-Host "Has Mojibake: $hasMojibake"

$approvalsResp = Invoke-WebRequest -Uri "http://localhost:5116/Home/Approvals" -WebSession $session -UseBasicParsing
$hasApprovalBanner = $approvalsResp.Content.Contains("Duyệt bảng lương định kỳ")
Write-Host "Approvals page has Payroll banner: $hasApprovalBanner"

