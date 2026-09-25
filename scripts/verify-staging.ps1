param(
    [Parameter(Mandatory=$true)][string]$BaseUrl
)

$ErrorActionPreference = 'Stop'
$root = $BaseUrl.TrimEnd('/')
$health = Invoke-RestMethod -Uri "$root/Health" -Method Get -TimeoutSec 30
$databaseResponse = Invoke-WebRequest -Uri "$root/Health/Database" -Method Get -TimeoutSec 30 -SkipHttpErrorCheck
if ($databaseResponse.StatusCode -ne 200) {
    throw "Staging database health check failed with HTTP $($databaseResponse.StatusCode): $($databaseResponse.Content)"
}
Write-Host "PASS application health: $health"
Write-Host "PASS database health: $($databaseResponse.Content)"
