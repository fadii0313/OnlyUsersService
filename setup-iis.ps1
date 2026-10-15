# ==============================================================================
# OnlyUsersService - Automated IIS Deployment & Provisioning Script
# Port: 5200 | Target: OnlyUsersService_API | Database: OnlyUsersService_DB (.\SQLEXPRESS)
# ==============================================================================

[CmdletBinding()]
param(
    [string]$SiteName = "OnlyUsersService_API",
    [string]$AppPoolName = "OnlyUsersService_AppPool",
    [int]$Port = 5200,
    [string]$PublishPath = "D:\ONLYUSERSSERVICE\publish",
    [string]$ProjectPath = "D:\ONLYUSERSSERVICE\OnlyUsersService.API\OnlyUsersService.API.csproj"
)

Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host " Starting IIS Deployment for OnlyUsersService on Port $Port..." -ForegroundColor Cyan
Write-Host "=====================================================================" -ForegroundColor Cyan

# Check Administrator Rights
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "This script must be executed in an elevated Administrator PowerShell prompt!"
    exit 1
}

# 1. Check IIS Feature & WebAdministration Module
Write-Host "[1/6] Verifying IIS prerequisites..." -ForegroundColor Yellow
if ((Get-Service -Name W3SVC -ErrorAction SilentlyContinue).Status -ne "Running") {
    Start-Service W3SVC
}
Import-Module WebAdministration

# 2. Check AspNetCoreModuleV2
$aspNetCoreModule = Get-WebGlobalModule -Name AspNetCoreModuleV2 -ErrorAction SilentlyContinue
if ($null -eq $aspNetCoreModule) {
    Write-Warning "AspNetCoreModuleV2 was not detected in IIS Global Modules. Please install the ASP.NET Core Hosting Bundle 8.0."
} else {
    Write-Host "  -> AspNetCoreModuleV2 detected: $($aspNetCoreModule.Image)" -ForegroundColor Green
}

# 3. Publish Release Artifacts
Write-Host "[2/6] Publishing Release binaries via dotnet publish..." -ForegroundColor Yellow
dotnet publish "$ProjectPath" -c Release -o "$PublishPath"
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed with exit code $LASTEXITCODE."
    exit $LASTEXITCODE
}
Write-Host "  -> Build and publish completed successfully." -ForegroundColor Green

# 4. Configure NTFS Permissions and Directories
Write-Host "[3/6] Setting NTFS directory permissions on $PublishPath..." -ForegroundColor Yellow
$logsPath = Join-Path $PublishPath "logs"
if (-not (Test-Path $logsPath)) {
    New-Item -ItemType Directory -Path $logsPath -Force | Out-Null
}

# Grant IIS_IUSRS read/execute & Network Service modify/write
icacls "$PublishPath" /grant "IIS_IUSRS:(OI)(CI)RX" /T /Q | Out-Null
icacls "$PublishPath" /grant "NT AUTHORITY\NETWORK SERVICE:(OI)(CI)M" /T /Q | Out-Null
icacls "$logsPath" /grant "NT AUTHORITY\NETWORK SERVICE:(OI)(CI)F" /T /Q | Out-Null
Write-Host "  -> Permissions configured for IIS_IUSRS and NT AUTHORITY\NETWORK SERVICE." -ForegroundColor Green

# 5. Provision IIS Application Pool
Write-Host "[4/6] Provisioning IIS App Pool: $AppPoolName..." -ForegroundColor Yellow
if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) {
    New-Item "IIS:\AppPools\$AppPoolName" | Out-Null
    Write-Host "  -> Created new AppPool: $AppPoolName" -ForegroundColor Green
} else {
    Write-Host "  -> Existing AppPool found: $AppPoolName" -ForegroundColor Cyan
}

Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "processModel.identityType" -Value "NetworkService"
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "processModel.loadUserProfile" -Value $true
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "processModel.idleTimeout" -Value ([TimeSpan]::Zero)

# 6. Provision IIS Website
Write-Host "[5/6] Provisioning IIS Website: $SiteName on port $Port..." -ForegroundColor Yellow
if (Test-Path "IIS:\Sites\$SiteName") {
    Write-Host "  -> Stopping existing site for safe update..." -ForegroundColor Cyan
    Stop-WebSite -Name $SiteName -ErrorAction SilentlyContinue
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name "physicalPath" -Value $PublishPath
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name "applicationPool" -Value $AppPoolName
} else {
    New-WebSite -Name $SiteName -Port $Port -PhysicalPath $PublishPath -ApplicationPool $AppPoolName | Out-Null
    Write-Host "  -> Created new IIS Website: $SiteName" -ForegroundColor Green
}

# Start Pool & Site
Start-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
Start-WebSite -Name $SiteName -ErrorAction SilentlyContinue
Write-Host "  -> AppPool and Website started." -ForegroundColor Green

# 7. Health & Verification Check
Write-Host "[6/6] Verifying live HTTP response..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

try {
    $response = Invoke-WebRequest -Uri "http://localhost:$Port/swagger/v1/swagger.json" -UseBasicParsing -TimeoutSec 15
    if ($response.StatusCode -eq 200) {
        Write-Host "=====================================================================" -ForegroundColor Green
        Write-Host " [SUCCESS] OnlyUsersService is LIVE on IIS!" -ForegroundColor Green
        Write-Host " Swagger UI:       http://localhost:$Port/swagger" -ForegroundColor Cyan
        Write-Host " Frontend SPA:     http://localhost:$Port/" -ForegroundColor Cyan
        Write-Host " API Documentation: http://localhost:$Port/swagger/v1/swagger.json" -ForegroundColor Cyan
        Write-Host "=====================================================================" -ForegroundColor Green
    } else {
        Write-Warning "Received HTTP Status Code: $($response.StatusCode)"
    }
} catch {
    Write-Error "Failed to reach live service: $_"
}
