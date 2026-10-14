[CmdletBinding(SupportsShouldProcess)]
param(
    [switch]$RemoveData
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$composeFile = Join-Path $projectRoot 'compose.keycloak.yml'
$environmentFile = Join-Path $projectRoot 'infra\keycloak\.env'
$environmentExample = Join-Path $projectRoot 'infra\keycloak\.env.example'

if (Test-Path -LiteralPath $environmentFile) {
    $composeEnvironmentFile = $environmentFile
}
else {
    $composeEnvironmentFile = $environmentExample
}

$arguments = @('compose', '--env-file', $composeEnvironmentFile, '-f', $composeFile, 'down', '--remove-orphans')
if ($RemoveData) {
    if (-not $PSCmdlet.ShouldProcess('Keycloak local PostgreSQL volume', 'Remove persistent identity data')) {
        return
    }

    $arguments += '--volumes'
}

& docker @arguments
if ($LASTEXITCODE -ne 0) {
    throw 'Không thể dừng Keycloak bằng Docker Compose.'
}

if ($RemoveData) {
    Write-Host 'Đã dừng Keycloak và xoá dữ liệu Keycloak cục bộ.'
}
else {
    Write-Host 'Đã dừng Keycloak. Dữ liệu Keycloak cục bộ được giữ lại.'
}
