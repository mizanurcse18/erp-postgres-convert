# 🚀 Opseek ERP - Quick Start Summary

## 📋 Current Status
✅ **Services Compiled**: API Gateway, Security API, Approval API, HRMS API, Mail API  
✅ **Runtime Target**: .NET Core 3.1  
✅ **Deployment Package**: Ready in `Published/` folder  
✅ **Scripts Available**: Automated setup scripts included  

---

## ⚡ 30-Second Setup (WSL)

### WSL Installation
```powershell
# 1. Copy files to WSL
wsl -- cp -r "/mnt/host/d/SourceCode/Opseek/source/ERP/erpservices/Published" /opt/erp-services/

# 2. Install .NET Core 3.1
wsl -- wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
wsl -- sudo dpkg -i packages-microsoft-prod.deb
wsl -- sudo apt update && sudo apt install -y aspnetcore-runtime-3.1

# 3. Run automated setup
wsl -- cd /opt/erp-services && chmod +x quick-start.sh && ./quick-start.sh setup
```

---

## 🎯 Service Endpoints

| Service | Port | URL | Purpose |
|---------|------|-----|---------|
| **API Gateway** | 5000 | http://localhost:5000 | Main entry point |
| **Security API** | 5001 | http://localhost:5001 | Authentication |
| **Approval API** | 5002 | http://localhost:5002 | Workflows |
| **HRMS API** | 5003 | http://localhost:5003 | Human Resources |
| **Mail API** | 5004 | http://localhost:5004 | Notifications |

---

## 🔧 Essential Commands

### Service Management (SystemD)
```bash
# Start all services
sudo systemctl start opseek-apigateway opseek-security opseek-approval

# Check status
sudo systemctl status opseek-apigateway opseek-security opseek-approval

# View logs
sudo journalctl -u opseek-apigateway -f

# Restart services
sudo systemctl restart opseek-apigateway opseek-security opseek-approval
```

### Manual Service Start (Development)
```bash
# Start API Gateway
cd /opt/erp-services/APIGateway
dotnet APIGateway.dll --urls "http://0.0.0.0:5000" &

# Start Security API
cd /opt/erp-services/Security.API
dotnet Security.API.dll --urls "http://0.0.0.0:5001" &

# Start Approval API
cd /opt/erp-services/Approval.API
dotnet Approval.API.dll --urls "http://0.0.0.0:5002" &
```

---

## 🔍 Health Checks

### Quick Status Check
```bash
# Check if services are running
ps aux | grep dotnet

# Check ports
netstat -tulpn | grep :500

# Test API response
curl http://localhost:5000
curl http://localhost:5001
curl http://localhost:5002
```

### Automated Health Script
```bash
# Run the included health check
sudo /usr/local/bin/opseek-health-check.sh

# Or create a simple one
for port in 5000 5001 5002; do
  echo -n "Port $port: "
  curl -s -o /dev/null -w "%{http_code}" http://localhost:$port && echo " ✅" || echo " ❌"
done
```

---

## 🔒 Configuration Essentials

### Database Connection (Edit appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=OpseekERP;User Id=user;Password=pass;"
  }
}
```

### Environment Variables
```bash
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5000
```

---

## 🚨 Troubleshooting

### Service Won't Start
```bash
# 1. Check .NET installation
dotnet --version

# 2. Check file permissions
ls -la /opt/erp-services/APIGateway/APIGateway.dll

# 3. Check logs
sudo journalctl -u opseek-apigateway --no-pager -n 50

# 4. Test manual start
cd /opt/erp-services/APIGateway
sudo dotnet APIGateway.dll
```

### Port Already in Use
```bash
# Find what's using the port
sudo netstat -tulpn | grep :5000

# Kill the process
sudo fuser -k 5000/tcp

# Restart service
sudo systemctl restart opseek-apigateway
```

### Database Connection Issues
```bash
# Check connection string in config
cat /opt/erp-services/Security.API/appsettings.json

# Test database connectivity
mysql -h your-server -u your-user -p your-database
```

---

## 📁 File Locations

### Services
- **Location**: `/opt/erp-services/`
- **Services**: `APIGateway/`, `Security.API/`, `Approval.API/`
- **Logs**: Each service directory + systemd logs

### Configuration
- **SystemD Services**: `/etc/systemd/system/opseek-*.service`
- **Nginx Config**: `/etc/nginx/sites-available/opseek-erp`
- **Health Scripts**: `/usr/local/bin/opseek-health-check.sh`

### Logs
- **Application Logs**: `/opt/erp-services/[ServiceName]/logs/`
- **System Logs**: `sudo journalctl -u opseek-[service]`
- **Nginx Logs**: `/var/log/nginx/`

---

## 📞 Getting Help

### Log Analysis Commands
```bash
# View recent errors
sudo journalctl --since "1 hour ago" | grep -i error

# Monitor logs in real-time  
sudo journalctl -u opseek-apigateway -f

# Check nginx errors
sudo tail -f /var/log/nginx/error.log
```

### Performance Monitoring
```bash
# Check memory usage
ps aux | grep dotnet | awk '{print $2, $4, $11}' | head -10

# Monitor system resources
htop

# Check disk space
df -h
```

---

## 🎯 Next Steps Checklist

### Immediate (Required)
- [ ] Install .NET Core 3.1 runtime
- [ ] Copy services to deployment location
- [ ] Update database connection strings
- [ ] Test service startup manually
- [ ] Configure firewall rules

### Production Setup (Recommended)
- [ ] Create systemd services
- [ ] Set up Nginx reverse proxy
- [ ] Configure SSL certificates
- [ ] Set up automated monitoring
- [ ] Create backup scripts
- [ ] Configure log rotation

### Additional Services (Optional)
- [ ] Deploy HRMS API (5003)
- [ ] Deploy SCM API (5005) 
- [ ] Deploy Accounts API (5006)
- [ ] Deploy Mail API (5004)
- [ ] Deploy remaining microservices

---

## ⚠️ Important Notes

### Security
- **🚨 .NET Core 3.1**: End of support December 2022 - plan upgrade to .NET 6+
- **🔒 Database**: Use strong passwords and secure connections
- **🛡️ Firewall**: Configure UFW for port access
- **🔐 SSL**: Use Let's Encrypt for production HTTPS

### Architecture
- **🏗️ Microservices**: Each service runs independently
- **🌐 API Gateway**: Single entry point using Ocelot
- **🔗 Dependencies**: Security API must start first
- **📊 Monitoring**: Health checks every 5 minutes via cron

---

## 📚 Documentation Files

1. **[COMPLETE-DEPLOYMENT-GUIDE.md](COMPLETE-DEPLOYMENT-GUIDE.md)** - Full detailed guide
2. **[Published/README.md](Published/README.md)** - Package overview
3. **[Published/ubuntu-deployment-guide.md](Published/ubuntu-deployment-guide.md)** - Ubuntu specific
4. **[Published/systemd-services-template.md](Published/systemd-services-template.md)** - Service templates
5. **[Published/quick-start.sh](Published/quick-start.sh)** - Automated setup script

---

**🎉 Your ERP system is ready for deployment!**

*Need help? Check the troubleshooting section or run the health check script.*
