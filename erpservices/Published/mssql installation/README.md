# SQL Server on WSL - Complete Documentation

## Overview
This folder contains comprehensive documentation for installing, configuring, and managing Microsoft SQL Server on Windows Subsystem for Linux (WSL) for the ERP system.

## Documentation Files

### 📋 [SQL-Server-WSL-Installation.md](SQL-Server-WSL-Installation.md)
**Complete installation guide from start to finish**
- Prerequisites and system requirements
- Step-by-step installation process
- Initial configuration and setup
- Service management commands
- Security configuration basics
- Performance optimization basics

### 🗄️ [Database-Setup.md](Database-Setup.md)
**Database creation, import, and configuration**
- Creating new databases
- Restoring from backup files
- Executing SQL scripts
- Setting up ERP-specific database users
- Connection string templates for all API services
- Database performance tuning
- Backup and recovery procedures

### 🔌 [Connectivity-Guide.md](Connectivity-Guide.md)
**Windows to WSL SQL Server connectivity**
- Network architecture overview
- Connection methods and best practices
- ERP application configuration
- Connection string examples
- SSL/TLS configuration
- Performance optimization
- Connection pooling setup
- Health checks implementation

### ⚙️ [SQL-Server-Configuration.md](SQL-Server-Configuration.md)
**Advanced configuration and optimization**
- Memory configuration
- Network settings
- Security hardening
- Performance tuning
- Agent and jobs setup
- Monitoring configuration
- Backup automation
- Maintenance procedures

### 🔧 [Troubleshooting.md](Troubleshooting.md)
**Problem diagnosis and resolution**
- Common issues and solutions
- Diagnostic commands and scripts
- Performance troubleshooting
- Connection problems
- Authentication issues
- Automated diagnostic tools
- Log collection procedures

## Quick Start

1. **Install SQL Server**: Follow [SQL-Server-WSL-Installation.md](SQL-Server-WSL-Installation.md)
2. **Set up Database**: Use [Database-Setup.md](Database-Setup.md) to create your ERP database
3. **Configure Connectivity**: Apply settings from [Connectivity-Guide.md](Connectivity-Guide.md)
4. **Optimize Performance**: Use [SQL-Server-Configuration.md](SQL-Server-Configuration.md) for advanced settings
5. **Handle Issues**: Refer to [Troubleshooting.md](Troubleshooting.md) when problems arise

## Installation Checklist

### Pre-Installation
- [ ] Windows 10/11 with WSL 2 enabled
- [ ] Ubuntu distribution installed
- [ ] At least 4GB RAM available
- [ ] 10GB free disk space
- [ ] Administrative privileges

### Installation Steps
- [ ] Update Ubuntu system
- [ ] Install SQL Server dependencies
- [ ] Add Microsoft repositories
- [ ] Install SQL Server package
- [ ] Run initial configuration
- [ ] Install command-line tools
- [ ] Configure PATH environment
- [ ] Test local connectivity

### Post-Installation
- [ ] Create ERP database
- [ ] Set up application users
- [ ] Configure connection strings
- [ ] Test connectivity from Windows
- [ ] Set up backup procedures
- [ ] Configure monitoring
- [ ] Apply security hardening

## Key Information

### Network Details
- **WSL IP Address**: Use `wsl hostname -I` to find current IP
- **SQL Server Port**: 1433 (default)
- **Connection Method**: Direct IP connection recommended

### Default Paths
- **Data Directory**: `/var/opt/mssql/data/`
- **Log Directory**: `/var/opt/mssql/log/`
- **Backup Directory**: `/var/opt/mssql/backup/`
- **Configuration**: `/var/opt/mssql/mssql.conf`

### ERP Services
- **Accounts.API**: Port 5001
- **Security.API**: Port 5002  
- **HRMS.API**: Port 5003
- **SCM.API**: Port 5004
- **Approval.API**: Port 5005
- **Mail.API**: Port 5006
- **APIGateway**: Port 5000

### Connection String Template
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server={{WSL_IP}};Database=ERPDatabase;User Id={{USERNAME}};Password={{PASSWORD}};TrustServerCertificate=true;Timeout=30;"
  }
}
```

## Support and Resources

### Microsoft Documentation
- [SQL Server on Linux](https://docs.microsoft.com/en-us/sql/linux/)
- [WSL Documentation](https://docs.microsoft.com/en-us/windows/wsl/)

### Community Resources
- Stack Overflow: `[sql-server]` + `[windows-subsystem-for-linux]`
- Microsoft Q&A: SQL Server section
- GitHub: SQL Server Docker and Linux issues

### Useful Commands

#### Quick Status Check
```bash
# Check WSL and SQL Server status
wsl --version
wsl hostname -I
wsl sudo systemctl status mssql-server --no-pager
wsl sqlcmd -S localhost -U sa -Q "SELECT @@VERSION"
```

#### Emergency Commands  
```bash
# Restart SQL Server
wsl sudo systemctl restart mssql-server

# Check error logs
wsl sudo tail -f /var/opt/mssql/log/errorlog

# Free up space
wsl sudo apt-get autoremove
wsl sudo apt-get autoclean
```

## File Structure

```
mssql installation/
├── README.md                           # This overview file
├── SQL-Server-WSL-Installation.md      # Complete installation guide
├── Database-Setup.md                   # Database creation and setup
├── Connectivity-Guide.md               # Windows to WSL connectivity
├── SQL-Server-Configuration.md         # Advanced configuration
└── Troubleshooting.md                  # Problem resolution guide
```

## Version Information

- **Documentation Version**: 1.0
- **Target SQL Server**: 2022 Developer Edition
- **Target Platform**: WSL 2 Ubuntu
- **Target Framework**: .NET 6+ ERP APIs
- **Created**: October 2025
- **Last Updated**: October 2025

---

**Note**: Always ensure you have proper backups before making configuration changes to your SQL Server installation.

**Tip**: Bookmark this README.md file for quick access to all MSSQL documentation.