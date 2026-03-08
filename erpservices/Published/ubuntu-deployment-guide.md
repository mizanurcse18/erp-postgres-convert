# Nagad ERP Services - Ubuntu Deployment Guide

## Overview
Your .NET Core 3.1 microservices are ready for deployment to Ubuntu. The build was successful and all compilation issues have been resolved.

## Services Published
- **APIGateway** - Main entry point for all requests
- **Security.API** - Authentication and authorization service
- **Approval.API** - Approval workflows and processes

## Prerequisites for Ubuntu Server

### 1. Install .NET Core 3.1 Runtime
```bash
# Download and install Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Update package list
sudo apt-get update

# Install ASP.NET Core 3.1 runtime
sudo apt-get install -y aspnetcore-runtime-3.1

# Install .NET 5.0 runtime (for Oracle.API if you publish it later)
sudo apt-get install -y aspnetcore-runtime-5.0

# Verify installation
dotnet --info
```

### 2. Create Application Directory
```bash
sudo mkdir -p /opt/nagad-services
sudo chown $USER:$USER /opt/nagad-services
```

## Deployment Steps

### 1. Copy Files to Ubuntu Server
Upload your `Published` folder to your Ubuntu server. You can use:
- SCP: `scp -r Published/ user@your-server:/opt/nagad-services/`
- SFTP or any file transfer tool
- Git if you commit the Published folder

### 2. Set Permissions
```bash
cd /opt/nagad-services
find . -name "*.dll" -exec chmod +x {} \;
```

### 3. Test Individual Services
```bash
# Test API Gateway
cd /opt/nagad-services/APIGateway
dotnet APIGateway.dll

# Test Security API (in new terminal)
cd /opt/nagad-services/Security.API
dotnet Security.API.dll

# Test Approval API (in new terminal)
cd /opt/nagad-services/Approval.API
dotnet Approval.API.dll
```

## Configuration

### Update Connection Strings
Before running, update your `appsettings.json` files in each service with your Ubuntu server database connections:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-db-server;Database=your-db;User Id=your-user;Password=your-password;"
  }
}
```

### Environment Variables (Optional)
```bash
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000
```

## Running Services in Background

### Option 1: Using nohup
```bash
# API Gateway (Port 5000)
cd /opt/nagad-services/APIGateway
nohup dotnet APIGateway.dll > apigateway.log 2>&1 &

# Security API (Port 5001)
cd /opt/nagad-services/Security.API
ASPNETCORE_URLS=http://0.0.0.0:5001 nohup dotnet Security.API.dll > security.log 2>&1 &

# Approval API (Port 5002)
cd /opt/nagad-services/Approval.API
ASPNETCORE_URLS=http://0.0.0.0:5002 nohup dotnet Approval.API.dll > approval.log 2>&1 &
```

### Option 2: Using systemd (Recommended for Production)
Create systemd service files for automatic startup and management.

#### Example: API Gateway Service
Create `/etc/systemd/system/nagad-apigateway.service`:
```ini
[Unit]
Description=Nagad API Gateway Service
After=network.target

[Service]
Type=simple
User=www-data
WorkingDirectory=/opt/nagad-services/APIGateway
ExecStart=/usr/bin/dotnet /opt/nagad-services/APIGateway/APIGateway.dll
Restart=on-failure
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=nagad-apigateway
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000

[Install]
WantedBy=multi-user.target
```

Then enable and start the service:
```bash
sudo systemctl daemon-reload
sudo systemctl enable nagad-apigateway
sudo systemctl start nagad-apigateway
sudo systemctl status nagad-apigateway
```

## Nginx Reverse Proxy (Optional)

If you want to use Nginx as a reverse proxy:

### Install Nginx
```bash
sudo apt-get install nginx
```

### Configure Nginx
Create `/etc/nginx/sites-available/nagad-services`:
```nginx
server {
    listen 80;
    server_name your-domain.com;

    location /api/gateway {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /api/security {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    location /api/approval {
        proxy_pass http://localhost:5002;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

Enable the configuration:
```bash
sudo ln -s /etc/nginx/sites-available/nagad-services /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

## Security Considerations

1. **Firewall**: Configure UFW to only allow necessary ports
2. **SSL/TLS**: Use Let's Encrypt or your SSL certificates
3. **Database**: Secure your database connections
4. **Logs**: Monitor application logs regularly

## Monitoring Commands

```bash
# Check service status
sudo systemctl status nagad-apigateway
sudo systemctl status nagad-security
sudo systemctl status nagad-approval

# View logs
sudo journalctl -u nagad-apigateway -f
tail -f /opt/nagad-services/APIGateway/apigateway.log

# Check running processes
ps aux | grep dotnet
netstat -tulpn | grep :500
```

## Troubleshooting

### Common Issues:
1. **Port conflicts**: Make sure each service uses different ports
2. **Database connections**: Verify connection strings are correct
3. **Permissions**: Ensure proper file permissions
4. **Dependencies**: Check all .NET dependencies are installed

### Logs Location:
- Application logs: `/opt/nagad-services/[ServiceName]/logs/`
- System logs: `sudo journalctl -u [service-name]`

## Next Steps

1. **Publish remaining services**: Use the same pattern to publish other APIs
2. **Set up monitoring**: Consider using tools like Prometheus/Grafana
3. **Backup strategy**: Implement regular backups
4. **CI/CD**: Set up automated deployment pipeline

## Important Notes

⚠️ **Security Warning**: .NET Core 3.1 reached end of support in December 2022. Consider upgrading to .NET 6 or later for continued security updates.

✅ **Success**: All compilation errors have been fixed and services are ready for deployment!
