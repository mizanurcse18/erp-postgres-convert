# Systemd Service Templates for Nagad ERP Services

## Overview
These are template files for creating systemd services for production deployment. Copy these to `/etc/systemd/system/` on your Ubuntu server.

## 1. API Gateway Service

**File**: `/etc/systemd/system/nagad-apigateway.service`

```ini
[Unit]
Description=Nagad API Gateway Service
After=network.target

[Service]
Type=simple
User=www-data
Group=www-data
WorkingDirectory=/opt/nagad-services/APIGateway
ExecStart=/usr/bin/dotnet /opt/nagad-services/APIGateway/APIGateway.dll
Restart=on-failure
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=nagad-apigateway
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
Environment=DOTNET_ROOT=/usr/lib/dotnet

[Install]
WantedBy=multi-user.target
```

## 2. Security API Service

**File**: `/etc/systemd/system/nagad-security.service`

```ini
[Unit]
Description=Nagad Security API Service
After=network.target

[Service]
Type=simple
User=www-data
Group=www-data
WorkingDirectory=/opt/nagad-services/Security.API
ExecStart=/usr/bin/dotnet /opt/nagad-services/Security.API/Security.API.dll
Restart=on-failure
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=nagad-security
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5001
Environment=DOTNET_ROOT=/usr/lib/dotnet

[Install]
WantedBy=multi-user.target
```

## 3. Approval API Service

**File**: `/etc/systemd/system/nagad-approval.service`

```ini
[Unit]
Description=Nagad Approval API Service
After=network.target

[Service]
Type=simple
User=www-data
Group=www-data
WorkingDirectory=/opt/nagad-services/Approval.API
ExecStart=/usr/bin/dotnet /opt/nagad-services/Approval.API/Approval.API.dll
Restart=on-failure
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=nagad-approval
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5002
Environment=DOTNET_ROOT=/usr/lib/dotnet

[Install]
WantedBy=multi-user.target
```

## Setup Commands

After creating the service files, run these commands:

```bash
# Reload systemd daemon
sudo systemctl daemon-reload

# Enable services (auto-start on boot)
sudo systemctl enable nagad-apigateway
sudo systemctl enable nagad-security
sudo systemctl enable nagad-approval

# Start services
sudo systemctl start nagad-apigateway
sudo systemctl start nagad-security
sudo systemctl start nagad-approval

# Check status
sudo systemctl status nagad-apigateway
sudo systemctl status nagad-security
sudo systemctl status nagad-approval
```

## Management Commands

```bash
# Start all services
sudo systemctl start nagad-apigateway nagad-security nagad-approval

# Stop all services
sudo systemctl stop nagad-apigateway nagad-security nagad-approval

# Restart all services
sudo systemctl restart nagad-apigateway nagad-security nagad-approval

# View logs
sudo journalctl -u nagad-apigateway -f
sudo journalctl -u nagad-security -f
sudo journalctl -u nagad-approval -f

# View all service logs combined
sudo journalctl -u nagad-apigateway -u nagad-security -u nagad-approval -f
```

## Environment Configuration

For different environments, you can create override files:

### Development Override
**File**: `/etc/systemd/system/nagad-apigateway.service.d/development.conf`

```ini
[Service]
Environment=ASPNETCORE_ENVIRONMENT=Development
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
```

### Production Override
**File**: `/etc/systemd/system/nagad-apigateway.service.d/production.conf`

```ini
[Service]
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
Environment=ConnectionStrings__DefaultConnection="your-production-connection-string"
```

## Security Considerations

1. **User Permissions**: Services run as `www-data` user for security
2. **File Permissions**: Ensure service files have correct permissions
3. **Network Access**: Configure firewall rules for the ports
4. **Log Rotation**: Configure logrotate for application logs

## Monitoring

Set up monitoring for your services:

```bash
# Create a simple monitoring script
cat > /usr/local/bin/nagad-health-check.sh << 'EOF'
#!/bin/bash
services=("nagad-apigateway" "nagad-security" "nagad-approval")
for service in "${services[@]}"; do
    if systemctl is-active --quiet $service; then
        echo "$service: ✅ Running"
    else
        echo "$service: ❌ Down"
        # Optionally restart the service
        # sudo systemctl restart $service
    fi
done
EOF

chmod +x /usr/local/bin/nagad-health-check.sh
```

Add to crontab for regular monitoring:
```bash
# Check every 5 minutes
*/5 * * * * /usr/local/bin/nagad-health-check.sh >> /var/log/nagad-health.log 2>&1
```

## Backup and Updates

For updates and backups:

```bash
# Stop services before update
sudo systemctl stop nagad-apigateway nagad-security nagad-approval

# Backup current deployment
sudo cp -r /opt/nagad-services /opt/nagad-services-backup-$(date +%Y%m%d)

# Deploy new version (copy new files)

# Start services after update
sudo systemctl start nagad-apigateway nagad-security nagad-approval

# Verify all services are running
sudo systemctl status nagad-apigateway nagad-security nagad-approval
```
