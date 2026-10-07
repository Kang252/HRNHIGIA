[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [uri]$BaseUrl,

    [ValidateRange(1, 60)]
    [int]$TimeoutSeconds = 15,

    [switch]$AllowHttpForLocalTest
)

$schemeIsSecure = $BaseUrl.Scheme -eq 'https'
if (-not $schemeIsSecure -and -not $AllowHttpForLocalTest) {
    throw 'BaseUrl phải dùng HTTPS. Chỉ dùng -AllowHttpForLocalTest với loopback khi kiểm thử cục bộ.'
}

if (-not $schemeIsSecure -and -not $BaseUrl.IsLoopback) {
    throw 'HTTP chỉ được phép cho địa chỉ loopback khi kiểm thử cục bộ.'
}

$apiBase = $BaseUrl.AbsoluteUri.TrimEnd('/') + '/api/v1'
$healthUrl = "$apiBase/health"
$protectedUrl = "$apiBase/remote-attendance?fromDate=2026-10-01&toDate=2026-10-07"

try {
    $health = Invoke-RestMethod -Uri $healthUrl -Method Get -TimeoutSec $TimeoutSeconds -MaximumRedirection 0
}
catch {
    throw "Không gọi được health check của Nest API: $($_.Exception.Message)"
}

if (-not $health.success -or $health.status -ne 'ok') {
    throw 'Health check không trả trạng thái ok.'
}

$sql = $health.dependencies.sqlServer
if (-not $sql.configured -or -not $sql.connected) {
    throw 'Nest API chưa kết nối được SQL Server.'
}

if (-not $sql.apiSessionSchema.required -or -not $sql.apiSessionSchema.ready) {
    throw 'Schema HrmApiSession chưa sẵn sàng.'
}

$protectedStatus = $null
try {
    $response = Invoke-WebRequest -Uri $protectedUrl -Method Get -TimeoutSec $TimeoutSeconds -MaximumRedirection 0 -UseBasicParsing
    $protectedStatus = [int]$response.StatusCode
}
catch {
    if ($_.Exception.Response) {
        $protectedStatus = [int]$_.Exception.Response.StatusCode
    }
    else {
        throw "Không kiểm tra được route có bảo vệ: $($_.Exception.Message)"
    }
}

if ($protectedStatus -ne 401) {
    throw "Route có bảo vệ phải từ chối khi chưa đăng nhập (mong đợi 401, nhận $protectedStatus)."
}

Write-Host "Nest API sẵn sàng: SQL kết nối, HrmApiSession đủ schema, route bảo vệ trả 401 khi chưa đăng nhập."
