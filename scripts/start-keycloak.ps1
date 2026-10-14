[CmdletBinding()]
param(
    [ValidateRange(10, 300)]
    [int]$TimeoutSeconds = 120
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$composeFile = Join-Path $projectRoot 'compose.keycloak.yml'
$environmentFile = Join-Path $projectRoot 'infra\keycloak\.env'
$environmentExample = Join-Path $projectRoot 'infra\keycloak\.env.example'

if (-not (Test-Path -LiteralPath $environmentFile)) {
    Copy-Item -LiteralPath $environmentExample -Destination $environmentFile
    throw "Đã tạo $environmentFile. Hãy thay KEYCLOAK_DB_PASSWORD và KEYCLOAK_ADMIN_PASSWORD trước khi chạy lại script."
}

function Get-EnvironmentFileValue {
    param([Parameter(Mandatory)][string]$Name)

    $match = Get-Content -LiteralPath $environmentFile |
        Where-Object { $_ -match "^$([regex]::Escape($Name))=(.+)$" } |
        Select-Object -First 1

    if (-not $match) {
        throw "Thiếu biến $Name trong $environmentFile."
    }

    return ($match -replace "^$([regex]::Escape($Name))=", '').Trim()
}

$databasePassword = Get-EnvironmentFileValue 'KEYCLOAK_DB_PASSWORD'
$adminPassword = Get-EnvironmentFileValue 'KEYCLOAK_ADMIN_PASSWORD'
if ($databasePassword -like 'change-this*' -or $adminPassword -like 'change-this*') {
    throw 'Hãy thay các mật khẩu mặc định trong infra/keycloak/.env trước khi khởi động Keycloak.'
}

$port = Get-EnvironmentFileValue 'KEYCLOAK_HTTP_PORT'
if ($port -notmatch '^\d+$') {
    throw 'KEYCLOAK_HTTP_PORT phải là một cổng hợp lệ.'
}

& docker version --format '{{.Server.Version}}' | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw 'Docker Desktop chưa chạy hoặc Docker daemon không khả dụng.'
}

& docker compose --env-file $environmentFile -f $composeFile up --detach --build --remove-orphans
if ($LASTEXITCODE -ne 0) {
    throw 'Không thể khởi động Keycloak bằng Docker Compose.'
}

$discoveryUrl = "http://localhost:$port/realms/nhigia/.well-known/openid-configuration"
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
do {
    try {
        $discovery = Invoke-RestMethod -Uri $discoveryUrl -Method Get -TimeoutSec 5
        if ($discovery.issuer -eq "http://localhost:$port/realms/nhigia") {
            Write-Host "Keycloak sẵn sàng: $discoveryUrl"
            Write-Host 'Client cục bộ: hrm-web-local'
            exit 0
        }
    }
    catch {
        # Keycloak is still starting or importing the realm.
    }

    Start-Sleep -Seconds 2
} while ((Get-Date) -lt $deadline)

throw "Keycloak chưa sẵn sàng sau $TimeoutSeconds giây. Xem log bằng: docker compose --env-file $environmentFile -f $composeFile logs keycloak"
