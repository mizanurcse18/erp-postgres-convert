# 🚀 Opseek ERP Services - Complete WSL/Ubuntu Deployment Guide

## 📋 Table of Contents
1. [Project Overview](#project-overview)
2. [Architecture Overview](#architecture-overview)
3. [Prerequisites](#prerequisites)
4. [WSL Setup](#wsl-setup)
5. [Ubuntu Server Deployment](#ubuntu-server-deployment)
6. [Service Configuration](#service-configuration)
7. [Production Setup](#production-setup)
8. [Monitoring & Maintenance](#monitoring--maintenance)
9. [Security](#security)
10. [Troubleshooting](#troubleshooting)
11. [Additional Services](#additional-services)

---

## 📖 Project Overview

### 🏢 Opseek ERP Microservices
Your enterprise ERP system consists of multiple .NET Core 3.1 microservices designed for scalable business operations.

### ✅ Current Deployment Status
- **Build Status**: ✅ Successfully compiled, all errors resolved
- **Services Ready**: API Gateway, Security API, Approval API, HRMS API, Mail API
- **Target Platform**: Linux (Ubuntu/WSL)
- **Runtime**: .NET Core 3.1 / ASP.NET Core 3.1

### 📁 Project Structure
```
D:\SourceCode\Opseek\source\ERP\erpservices\
├── Core/                           # Shared libraries
│   ├── API.Core/                   # API base classes
│   ├── Core/                       # Common utilities
│   ├── DAL.Core/                   # Data access layer
│   └── Manager.Core/               # Business logic base
├── Gateways/
│   └── APIGateway/                 # Ocelot API Gateway
├── Services/
│   ├── Security/                   # Authentication & Authorization
│   ├── HRMS/                       # Human Resources Management
│   ├── Approval/                   # Approval Workflows
│   ├── SCM/                        # Supply Chain Management
│   ├── Accounts/                   # Financial Management
│   ├── Mail/                       # Email Services
│   ├── MicroSite/                  # Web Portal
│   ├── External/                   # Third-party Integrations
│   ├── DCM/                        # Document Management
│   ├── Oracle/                     # Oracle Integration (.NET 5.0)
│   └── Worker/                     # Background Services
└── Published/                      # Ready-to-deploy services
    ├── APIGateway/
    ├── Security.API/
    ├── Approval.API/
    ├── HRMS.API/
    ├── Mail.API/
    ├── quick-start.sh
    ├── ubuntu-deployment-guide.md
    └── systemd-services-template.md
```

---

## 🏗️ Architecture Overview

### 🌐 Microservices Architecture
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   API Gateway   │    │   Security API  │    │  Approval API   │
│    Port 5000    │◄───┤    Port 5001    │◄───┤    Port 5002    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 ▼
                    ┌─────────────────────────┐
                    │    Ubuntu Server        │
                    │  .NET Core 3.1 Runtime │
                    └─────────────────────────┘
```

### 🔧 Service Details
| Service | Port | Purpose | Dependencies |
|---------|------|---------|--------------|
| **API Gateway** | 5000 | Main entry point, routing | Ocelot |
| **Security API** | 5001 | Authentication, JWT tokens | SQL Server/MySQL |
| **Approval API** | 5002 | Business workflows | Security API |
| **HRMS API** | 5003 | Human resources | Security API |
| **Mail API** | 5004 | Email notifications | SMTP Server |
| **SCM API** | 5005 | Supply chain | Security API |
| **Accounts API** | 5006 | Financial data | SQL Server |
| **DCM API** | 5007 | Document management | File system |

---

## 📋 Prerequisites

### 💻 System Requirements
- **OS**: Ubuntu 20.04 LTS or Windows 11 with WSL2
- **RAM**: Minimum 4GB, Recommended 8GB+
- **Storage**: 10GB+ free space
- **Network**: Internet connection for package downloads

### 🔧 Software Dependencies
- .NET Core 3.1 Runtime & ASP.NET Core 3.1
- SQL Server/MySQL/PostgreSQL (database)
- Nginx (optional, for reverse proxy)
- systemd (for service management)

---

## 🖥️ WSL Setup

### 1. Install WSL and Ubuntu
```powershell
# Install WSL2 and Ubuntu 20.04
wsl --install Ubuntu-20.04

# Verify installation
wsl --list --verbose
```

### 2. Copy Project Files to WSL
```powershell
# Create directory in WSL
wsl -- mkdir -p ~/erp-backend

# Copy published services
wsl -- cp -r "/mnt/d/SourceCode/Opseek/source/ERP/erpservices/Published" ~/home/munnacse18/apps/erp
```

### 3. Install .NET Core 3.1 in WSL
```bash
# Enter WSL
wsl

# Download Microsoft packages
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET Core 3.1 runtime
sudo apt update
sudo apt install -y aspnetcore-runtime-3.1 dotnet-runtime-3.1

# Verify installation
dotnet --version
dotnet --info
```

### 4. Quick Start with Provided Scripts
```bash
# Navigate to Published directory
cd ~/erp-backend/Published

# Make script executable
chmod +x quick-start.sh

# Run automated setup
./quick-start.sh setup

# Or run specific actions
./quick-start.sh start    # Start all services
./quick-start.sh stop     # Stop all services
./quick-start.sh status   # Check service status
```

---

## 🌐 Ubuntu Server Deployment

### 1. Prepare Ubuntu Server
```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install required packages
sudo apt install -y curl wget apt-transport-https software-properties-common

# Install .NET Core 3.1
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update
sudo apt install -y aspnetcore-runtime-3.1 dotnet-runtime-3.1
```

### 2. Create Application Directory Structure
```bash
# Create main services directory
sudo mkdir -p /opt/erp-services
sudo chown $USER:$USER /opt/erp-services

# Create subdirectories for each service
mkdir -p /opt/erp-services/{APIGateway,Security.API,Approval.API,HRMS.API,Mail.API}

# Create logs directory
mkdir -p /opt/erp-services/logs
```

### 3. Copy and Configure Services
```bash
# Copy service files (from your deployment package)
cp -r APIGateway/* /opt/erp-services/APIGateway/
cp -r Security.API/* /opt/erp-services/Security.API/
cp -r Approval.API/* /opt/erp-services/Approval.API/
cp -r HRMS.API/* /opt/erp-services/HRMS.API/
cp -r Mail.API/* /opt/erp-services/Mail.API/

# Set executable permissions
find /opt/erp-services -name "*.dll" -exec chmod +x {} \;
```

---

## ⚙️ Service Configuration

### 1. Update Database Connection Strings
For each service, update the `appsettings.json` file:

#### API Gateway Configuration
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": "*",
  "GlobalConfiguration": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

#### Security API Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-db-server;Database=OpseekSecurity;User Id=your-user;Password=your-password;",
    "SecurityConnection": "Server=your-db-server;Database=OpseekSecurity;User Id=your-user;Password=your-password;"
  },
  "JWTSettings": {
    "SecretKey": "your-secret-key-min-32-characters",
    "Issuer": "OpseekERP",
    "Audience": "OpseekClients",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

#### Approval API Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-db-server;Database=OpseekApproval;User Id=your-user;Password=your-password;"
  },
  "SecurityAPI": {
    "BaseUrl": "http://localhost:5001"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### 2. Environment Variables
Create environment configuration files:

```bash
# Create environment file
sudo nano /opt/erp-services/.env
```

```bash
# .env file content
ASPNETCORE_ENVIRONMENT=Production
DOTNET_ROOT=/usr/lib/dotnet

# Database connections
DB_SERVER=your-database-server
DB_USERNAME=your-db-username
DB_PASSWORD=your-secure-password

# JWT Settings
JWT_SECRET_KEY=your-very-secure-secret-key-at-least-32-characters
JWT_ISSUER=OpseekERP
JWT_AUDIENCE=OpseekClients

# Service URLs
SECURITY_API_URL=http://localhost:5001
APPROVAL_API_URL=http://localhost:5002
HRMS_API_URL=http://localhost:5003
```

---

## 🏭 Production Setup

### 1. Create SystemD Services

#### API Gateway Service
```bash
sudo nano /etc/systemd/system/opseek-apigateway.service
```

```ini
[Unit]
Description=Opseek API Gateway Service
After=network.target

[Service]
Type=simple
User=www-data
Group=www-data
WorkingDirectory=/opt/erp-services/APIGateway
ExecStart=/usr/bin/dotnet /opt/erp-services/APIGateway/APIGateway.dll
Restart=on-failure
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=opseek-apigateway
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
Environment=DOTNET_ROOT=/usr/lib/dotnet
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
```

#### Security API Service
```bash
sudo nano /etc/systemd/system/opseek-security.service
```

```ini
[Unit]
Description=Opseek Security API Service
After=network.target

[Service]
Type=simple
User=www-data
Group=www-data
WorkingDirectory=/opt/erp-services/Security.API
ExecStart=/usr/bin/dotnet /opt/erp-services/Security.API/Security.API.dll
Restart=on-failure
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=opseek-security
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5001
Environment=DOTNET_ROOT=/usr/lib/dotnet
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
```

#### Approval API Service
```bash
sudo nano /etc/systemd/system/opseek-approval.service
```

```ini
[Unit]
Description=Opseek Approval API Service
After=network.target opseek-security.service

[Service]
Type=simple
User=www-data
Group=www-data
WorkingDirectory=/opt/erp-services/Approval.API
ExecStart=/usr/bin/dotnet /opt/erp-services/Approval.API/Approval.API.dll
Restart=on-failure
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=opseek-approval
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5002
Environment=DOTNET_ROOT=/usr/lib/dotnet
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
```

### 2. Enable and Start Services
```bash
# Reload systemd daemon
sudo systemctl daemon-reload

# Enable services (auto-start on boot)
sudo systemctl enable opseek-apigateway
sudo systemctl enable opseek-security
sudo systemctl enable opseek-approval

# Start services
sudo systemctl start opseek-apigateway
sudo systemctl start opseek-security
sudo systemctl start opseek-approval

# Check status
sudo systemctl status opseek-apigateway
sudo systemctl status opseek-security
sudo systemctl status opseek-approval
```

### 3. Nginx Reverse Proxy Setup
```bash
# Install Nginx
sudo apt install nginx

# Create Nginx configuration
sudo nano /etc/nginx/sites-available/opseek-erp
```

```nginx
upstream apigateway {
    server 127.0.0.1:5000;
}

upstream security_api {
    server 127.0.0.1:5001;
}

upstream approval_api {
    server 127.0.0.1:5002;
}

server {
    listen 80;
    server_name your-domain.com;

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;

    # API Gateway routes
    location / {
        proxy_pass http://apigateway;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Real-IP $remote_addr;
    }

    # Direct API access (if needed)
    location /api/security {
        proxy_pass http://security_api;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    location /api/approval {
        proxy_pass http://approval_api;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    # Health check endpoint
    location /health {
        access_log off;
        return 200 "healthy\n";
        add_header Content-Type text/plain;
    }
}
```

```bash
# Enable the site
sudo ln -s /etc/nginx/sites-available/opseek-erp /etc/nginx/sites-enabled/

# Test configuration
sudo nginx -t

# Reload Nginx
sudo systemctl reload nginx
```

---

## 📊 Monitoring & Maintenance

### 1. Service Health Monitoring
Create a health check script:

```bash
sudo nano /usr/local/bin/opseek-health-check.sh
```

```bash
#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

services=("opseek-apigateway" "opseek-security" "opseek-approval")
ports=(5000 5001 5002)

echo "==================================="
echo "Opseek ERP Services Health Check"
echo "==================================="
echo "$(date)"
echo ""

for i in "${!services[@]}"; do
    service_name="${services[$i]}"
    port="${ports[$i]}"
    
    # Check systemd service status
    if systemctl is-active --quiet $service_name; then
        echo -e "${service_name}: ${GREEN}✅ Running${NC}"
        
        # Check if port is listening
        if netstat -tuln | grep -q ":${port} "; then
            echo -e "  Port ${port}: ${GREEN}✅ Listening${NC}"
        else
            echo -e "  Port ${port}: ${RED}❌ Not listening${NC}"
        fi
        
        # Check HTTP response (if applicable)
        if curl -s -o /dev/null -w "%{http_code}" http://localhost:${port}/health | grep -q "200"; then
            echo -e "  Health: ${GREEN}✅ Healthy${NC}"
        else
            echo -e "  Health: ${YELLOW}⚠️ No health endpoint${NC}"
        fi
    else
        echo -e "${service_name}: ${RED}❌ Down${NC}"
        echo -e "  Attempting restart..."
        sudo systemctl restart $service_name
    fi
    echo ""
done

echo "==================================="
```

```bash
# Make executable
sudo chmod +x /usr/local/bin/opseek-health-check.sh

# Add to crontab for regular checks
(crontab -l 2>/dev/null; echo "*/5 * * * * /usr/local/bin/opseek-health-check.sh >> /var/log/opseek-health.log 2>&1") | crontab -
```

### 2. Log Management
```bash
# Create log rotation configuration
sudo nano /etc/logrotate.d/opseek-erp
```

```
/opt/erp-services/*/logs/*.log {
    daily
    missingok
    rotate 52
    compress
    delaycompress
    notifempty
    create 644 www-data www-data
    postrotate
        systemctl reload opseek-apigateway opseek-security opseek-approval
    endscript
}
```

### 3. Backup Script
```bash
sudo nano /usr/local/bin/opseek-backup.sh
```

```bash
#!/bin/bash

BACKUP_DIR="/backup/opseek-erp"
DATE=$(date +%Y%m%d_%H%M%S)
SERVICE_DIR="/opt/erp-services"

# Create backup directory
mkdir -p $BACKUP_DIR

# Stop services
echo "Stopping services..."
sudo systemctl stop opseek-apigateway opseek-security opseek-approval

# Create backup
echo "Creating backup..."
tar -czf "$BACKUP_DIR/opseek-services-$DATE.tar.gz" -C /opt erp-services

# Restart services
echo "Restarting services..."
sudo systemctl start opseek-apigateway opseek-security opseek-approval

# Clean old backups (keep 30 days)
find $BACKUP_DIR -name "*.tar.gz" -mtime +30 -delete

echo "Backup completed: opseek-services-$DATE.tar.gz"
```

```bash
# Make executable and schedule weekly backups
sudo chmod +x /usr/local/bin/opseek-backup.sh
(crontab -l 2>/dev/null; echo "0 2 * * 0 /usr/local/bin/opseek-backup.sh") | crontab -
```

---

## 🔒 Security

### 1. Firewall Configuration
```bash
# Install UFW firewall
sudo apt install ufw

# Default policies
sudo ufw default deny incoming
sudo ufw default allow outgoing

# Allow SSH
sudo ufw allow ssh

# Allow HTTP and HTTPS
sudo ufw allow http
sudo ufw allow https

# Allow specific service ports (if needed for direct access)
sudo ufw allow 5000/tcp comment 'API Gateway'
sudo ufw allow 5001/tcp comment 'Security API'
sudo ufw allow 5002/tcp comment 'Approval API'

# Enable firewall
sudo ufw enable

# Check status
sudo ufw status verbose
```

### 2. SSL/TLS with Let's Encrypt
```bash
# Install Certbot
sudo apt install snapd
sudo snap install --classic certbot

# Create symlink
sudo ln -s /snap/bin/certbot /usr/bin/certbot

# Generate SSL certificate
sudo certbot --nginx -d your-domain.com

# Test auto-renewal
sudo certbot renew --dry-run
```

### 3. Security Hardening
```bash
# Create dedicated user for services
sudo useradd -r -s /bin/false opseek-user
sudo chown -R opseek-user:opseek-user /opt/erp-services

# Update service files to use the new user
sudo sed -i 's/User=www-data/User=opseek-user/g' /etc/systemd/system/opseek-*.service
sudo sed -i 's/Group=www-data/Group=opseek-user/g' /etc/systemd/system/opseek-*.service

# Reload and restart
sudo systemctl daemon-reload
sudo systemctl restart opseek-apigateway opseek-security opseek-approval
```

---

## 🔧 Troubleshooting

### Common Issues and Solutions

#### 1. Services Won't Start
```bash
# Check service status
sudo systemctl status opseek-apigateway

# View detailed logs
sudo journalctl -u opseek-apigateway -f

# Check .NET installation
dotnet --info

# Verify file permissions
ls -la /opt/erp-services/APIGateway/APIGateway.dll

# Test manual startup
cd /opt/erp-services/APIGateway
sudo -u opseek-user dotnet APIGateway.dll
```

#### 2. Database Connection Issues
```bash
# Test database connectivity
sudo apt install mysql-client  # or postgresql-client
mysql -h your-db-server -u your-user -p

# Check connection strings in appsettings.json
cat /opt/erp-services/Security.API/appsettings.json | grep -A 5 "ConnectionStrings"

# View database-related errors
sudo journalctl -u opseek-security | grep -i "database\|connection\|sql"
```

#### 3. Port Conflicts
```bash
# Check which processes are using ports
sudo netstat -tulpn | grep -E ":(500[0-9])"

# Kill conflicting processes
sudo fuser -k 5000/tcp

# Restart services
sudo systemctl restart opseek-apigateway
```

#### 4. High Memory Usage
```bash
# Check memory usage by service
ps aux | grep dotnet | head -10

# Monitor in real-time
htop

# Restart services if needed
sudo systemctl restart opseek-apigateway opseek-security opseek-approval
```

### Diagnostic Commands
```bash
# System information
hostnamectl
df -h
free -h
cat /proc/cpuinfo | grep "model name" | head -1

# Service information
sudo systemctl list-units --type=service | grep opseek
sudo systemctl is-enabled opseek-apigateway opseek-security opseek-approval
sudo systemctl is-active opseek-apigateway opseek-security opseek-approval

# Network information
sudo netstat -tulpn | grep :500
curl -I http://localhost:5000
curl -I http://localhost:5001
curl -I http://localhost:5002

# Log analysis
sudo journalctl --since "1 hour ago" | grep opseek
tail -f /var/log/nginx/access.log
tail -f /var/log/nginx/error.log
```

---

## 🔄 Additional Services

### Publishing Remaining Services

You have several other services that can be deployed using the same pattern:

#### 1. HRMS API (Human Resources)
```bash
# Build and publish
dotnet publish Services/HRMS/HRMS.API/HRMS.API.csproj -c Release -f netcoreapp3.1 -r linux-x64 --self-contained false -o Published/HRMS.API

# Create systemd service
sudo nano /etc/systemd/system/opseek-hrms.service
```

#### 2. SCM API (Supply Chain)
```bash
# Build and publish
dotnet publish Services/SCM/SCM.API/SCM.API.csproj -c Release -f netcoreapp3.1 -r linux-x64 --self-contained false -o Published/SCM.API

# Port: 5005
```

#### 3. Accounts API (Financial)
```bash
# Build and publish
dotnet publish Services/Accounts/Accounts.API/Accounts.API.csproj -c Release -f netcoreapp3.1 -r linux-x64 --self-contained false -o Published/Accounts.API

# Port: 5006
```

#### 4. Mail API (Notifications)
```bash
# Build and publish
dotnet publish Services/Mail/Mail.API/Mail.API.csproj -c Release -f netcoreapp3.1 -r linux-x64 --self-contained false -o Published/Mail.API

# Port: 5004
```

### Service Dependency Order
When starting multiple services, follow this order:
1. **Security API** (5001) - Authentication foundation
2. **API Gateway** (5000) - Main entry point
3. **Mail API** (5004) - Notifications
4. **Approval API** (5002) - Business processes
5. **HRMS API** (5003) - Human resources
6. **Accounts API** (5006) - Financial data
7. **SCM API** (5005) - Supply chain

---

## 📞 Support & Maintenance

### Regular Maintenance Tasks
- **Weekly**: Review logs and performance
- **Monthly**: Update security patches
- **Quarterly**: Full system backup and disaster recovery test
- **Annually**: Security audit and dependency updates

### Performance Optimization
```bash
# Enable gzip compression in Nginx
sudo nano /etc/nginx/nginx.conf
# Add in http block:
gzip on;
gzip_vary on;
gzip_proxied any;
gzip_comp_level 6;
gzip_types text/plain text/css text/xml application/json application/javascript application/xml+rss text/javascript;

# Optimize .NET for production
export DOTNET_GCServer=1
export DOTNET_GCConcurrent=1
```

### Scaling Considerations
- **Load Balancing**: Use multiple instances behind Nginx
- **Database**: Implement read replicas and connection pooling
- **Caching**: Add Redis for session and data caching
- **CDN**: Use CDN for static content delivery

---

## ⚠️ Important Notices

### End of Life Warning
**⚠️ .NET Core 3.1 Support**: .NET Core 3.1 reached end-of-support in December 2022. Consider upgrading to .NET 6+ for:
- Continued security updates
- Performance improvements
- Long-term support (LTS)

### Migration Path
1. **Immediate**: Deploy current .NET Core 3.1 services
2. **Short-term**: Plan migration to .NET 6+ 
3. **Long-term**: Consider containerization with Docker/Kubernetes

---

## 📝 Conclusion

Your Opseek ERP Services are now ready for production deployment on Ubuntu/WSL. This guide provides:

✅ **Complete setup instructions**  
✅ **Production-ready configuration**  
✅ **Security best practices**  
✅ **Monitoring and maintenance procedures**  
✅ **Troubleshooting guides**  

### Next Steps
1. **Deploy core services** (Gateway, Security, Approval)
2. **Configure databases** and test connectivity
3. **Set up monitoring** and alerting
4. **Plan for remaining services** deployment
5. **Consider .NET 6+ migration** timeline

### Support
For deployment issues or questions:
1. Check the troubleshooting section
2. Review service logs: `sudo journalctl -u opseek-[service-name] -f`
3. Verify configuration files and connection strings
4. Test individual services manually

**🎉 Congratulations!** Your enterprise ERP system is ready for production deployment!

---

*Last updated: September 2025*  
*Document version: 1.0*
