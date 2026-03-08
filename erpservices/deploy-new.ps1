# PowerShell script to publish all microservices for Ubuntu deployment
Write-Host "Starting deployment preparation for Ubuntu..." -ForegroundColor Green

# Set output directory
$OutputDir = "D:\SourceCode\Opseek\source\ERP\erpservices\Published"

# Create output directory if it doesn't exist
if (!(Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force
}

# Define all the services to publish
$Services = @(
    @{Name = "APIGateway"; Path = "Gateways\APIGateway\APIGateway.csproj"; Framework = "netcoreapp3.1"},
    @{Name = "Security.API"; Path = "Services\Security\Security.API\Security.API.csproj"; Framework = "netcoreapp3.1"},
    @{Name = "HRMS.API"; Path = "Services\HRMS\HRMS.API\HRMS.API.csproj"; Framework = "netcoreapp3.1"},
    @{Name = "Approval.API"; Path = "Services\Approval\Approval.API\Approval.API.csproj"; Framework = "netcoreapp3.1"},
    @{Name = "SCM.API"; Path = "Services\SCM\SCM.API\SCM.API.csproj"; Framework = "netcoreapp3.1"},
    @{Name = "Accounts.API"; Path = "Services\Accounts\Accounts.API\Accounts.API.csproj"; Framework = "netcoreapp3.1"},
    @{Name = "Mail.API"; Path = "Services\Mail\Mail.API\Mail.API.csproj"; Framework = "netcoreapp3.1"},   
    @{Name = "WorkerService"; Path = "Services\Worker\WorkerService\WorkerService.csproj"; Framework = "netcoreapp3.1"}
)

Write-Host "`nPublishing services for Ubuntu (linux-x64)..." -ForegroundColor Yellow

foreach ($Service in $Services) {
    Write-Host "`nPublishing $($Service.Name)..." -ForegroundColor Cyan
    
    $ServiceOutputDir = Join-Path $OutputDir $Service.Name
    
    try {
        dotnet publish $Service.Path -c Release -f $Service.Framework -r linux-x64 --self-contained false -o $ServiceOutputDir
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Successfully published $($Service.Name)" -ForegroundColor Green
        } else {
            Write-Host "Failed to publish $($Service.Name)" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "Error publishing $($Service.Name): $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`nDeployment preparation completed!" -ForegroundColor Green
Write-Host "Published services are located in: $OutputDir" -ForegroundColor Yellow

# Generate Ubuntu deployment script
$UbuntuScript = @'
#!/bin/bash
# Ubuntu Deployment Commands

# Install .NET runtimes
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-3.1 aspnetcore-runtime-5.0

# Create application directory
sudo mkdir -p /home/munnacse18/apps/back-end-services
sudo chown $USER:$USER /home/munnacse18/apps/back-end-services

echo "Deployment setup completed!"
'@

$UbuntuScriptPath = Join-Path $OutputDir "ubuntu-deploy.sh"
$UbuntuScript | Out-File -FilePath $UbuntuScriptPath -Encoding UTF8

Write-Host "Ubuntu deployment script created: $UbuntuScriptPath" -ForegroundColor Green