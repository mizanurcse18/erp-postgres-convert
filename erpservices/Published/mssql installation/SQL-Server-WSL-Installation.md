# SQL Server on WSL - Complete Installation Guide

## Overview
This document provides detailed step-by-step instructions for installing Microsoft SQL Server on Windows Subsystem for Linux (WSL) and setting up database connectivity for the ERP application.

## Prerequisites Check

Before starting, ensure you have:
- ✅ Windows 10/11 with WSL 2 enabled
- ✅ Ubuntu distribution installed in WSL
- ✅ Administrative privileges
- ✅ At least 4GB RAM available
- ✅ 10GB free disk space

### Verify WSL Status
```powershell
# Run from Windows PowerShell
wsl --version
wsl --list --verbose
```

## Step-by-Step Installation

### Step 1: Update Ubuntu System

```bash
# Connect to WSL Ubuntu
wsl

# Update package repositories
sudo apt-get update
sudo apt-get upgrade -y
```

### Step 2: Install Required Dependencies

```bash
# Install curl and other dependencies
sudo apt-get install -y curl apt-transport-https gnupg lsb-release
```

### Step 3: Add Microsoft Repository

```bash
# Import Microsoft GPG key
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -

# Add Microsoft SQL Server repository for Ubuntu 20.04
sudo add-apt-repository "$(wget -qO- https://packages.microsoft.com/config/ubuntu/20.04/mssql-server-2022.list)"

# Update package cache
sudo apt-get update
```

### Step 4: Install SQL Server

```bash
# Install SQL Server package
sudo apt-get install -y mssql-server
```

### Step 5: Configure SQL Server

```bash
# Run SQL Server configuration setup
sudo /opt/mssql/bin/mssql-conf setup
```

**Configuration Prompts:**
1. **Edition Selection**: Choose `2` (Developer Edition - Free)
2. **License Agreement**: Type `Yes` to accept
3. **Language**: Press `Enter` for default (English)
4. **SA Password**: Enter a strong password
   - Minimum 8 characters
   - Must include: uppercase, lowercase, numbers, and special characters
   - Example: `MyStr0ng_P@ssw0rd`

### Step 6: Enable and Start SQL Server

```bash
# Enable SQL Server service to start on boot
sudo systemctl enable mssql-server

# Start SQL Server service
sudo systemctl start mssql-server

# Check service status
sudo systemctl status mssql-server --no-pager
```

**Expected Output:**
```
● mssql-server.service - Microsoft SQL Server Database Engine
     Loaded: loaded (/lib/systemd/system/mssql-server.service; enabled)
     Active: active (running)
```

### Step 7: Install SQL Server Command-Line Tools

```bash
# Add Microsoft repository for tools
curl https://packages.microsoft.com/config/ubuntu/20.04/prod.list | sudo tee /etc/apt/sources.list.d/msprod.list

# Update package cache
sudo apt-get update

# Install sqlcmd and bcp utilities
sudo apt-get install -y mssql-tools unixodbc-dev

# Accept EULA during installation
# Choose "Yes" when prompted for both mssql-tools and unixodbc-dev
```

### Step 8: Configure PATH Environment

```bash
# Add SQL Server tools to PATH
echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc
echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bash_profile

# Reload bash configuration
source ~/.bashrc
```

## Verification and Testing

### Test 1: Local Connection

```bash
# Connect to SQL Server locally
sqlcmd -S localhost -U sa

# Enter your SA password when prompted
# Run a test query
SELECT @@VERSION;
GO

# Exit sqlcmd
quit
```

### Test 2: Check SQL Server Process

```bash
# Check if SQL Server process is running
ps aux | grep mssql

# Check network listening ports
sudo netstat -tuln | grep 1433
```

### Test 3: Get WSL Network Information

```bash
# Get WSL IP address
hostname -I

# Show network configuration
ip addr show
```

## Windows Connectivity Setup

### Step 1: Find WSL IP Address

From WSL Ubuntu:
```bash
# Get the primary IP address
hostname -I | awk '{print $1}'
```

### Step 2: Test Connection from Windows

From Windows Command Prompt/PowerShell:
```cmd
# Test network connectivity
ping [WSL_IP_ADDRESS]

# Test SQL Server connection (install sqlcmd on Windows if needed)
sqlcmd -S [WSL_IP_ADDRESS] -U sa
```

### Step 3: Connection String for Applications

For .NET applications, use this connection string format:
```csharp
"Server=[WSL_IP_ADDRESS];Database=[DATABASE_NAME];User Id=sa;Password=[SA_PASSWORD];TrustServerCertificate=true;"
```

Example:
```csharp
"Server=192.168.68.113;Database=ERPDatabase;User Id=sa;Password=MyStr0ng_P@ssw0rd;TrustServerCertificate=true;"
```

## Security Configuration

### Create Application User (Recommended)

Instead of using SA account for applications:

```sql
-- Connect as SA
sqlcmd -S localhost -U sa

-- Create a new login for the application
CREATE LOGIN ERPUser WITH PASSWORD = 'ERP_P@ssw0rd123';
GO

-- Create database if it doesn't exist
CREATE DATABASE ERPDatabase;
GO

-- Use the database
USE ERPDatabase;
GO

-- Create user and assign permissions
CREATE USER ERPUser FOR LOGIN ERPUser;
GO

-- Add user to database roles
ALTER ROLE db_datareader ADD MEMBER ERPUser;
ALTER ROLE db_datawriter ADD MEMBER ERPUser;
ALTER ROLE db_ddladmin ADD MEMBER ERPUser;
GO

quit
```

### Updated Connection String with Application User
```csharp
"Server=[WSL_IP_ADDRESS];Database=ERPDatabase;User Id=ERPUser;Password=ERP_P@ssw0rd123;TrustServerCertificate=true;"
```

## Service Management Commands

### Start/Stop/Restart SQL Server
```bash
# Stop SQL Server
sudo systemctl stop mssql-server

# Start SQL Server
sudo systemctl start mssql-server

# Restart SQL Server
sudo systemctl restart mssql-server

# Check status
sudo systemctl status mssql-server --no-pager
```

### Enable/Disable Auto-start
```bash
# Enable auto-start on boot
sudo systemctl enable mssql-server

# Disable auto-start
sudo systemctl disable mssql-server
```

## Troubleshooting

### Issue 1: Service Won't Start
```bash
# Check detailed error logs
sudo journalctl -u mssql-server -f

# Check SQL Server error log
sudo tail -f /var/opt/mssql/log/errorlog
```

### Issue 2: Cannot Connect from Windows
- Verify WSL IP address: `hostname -I`
- Check if SQL Server is listening: `sudo netstat -tuln | grep 1433`
- Test network connectivity: `ping [WSL_IP]` from Windows
- Ensure Windows Firewall allows the connection

### Issue 3: Password Policy Issues
SQL Server password must meet these requirements:
- At least 8 characters long
- Contains characters from at least 3 of these 4 categories:
  - Uppercase letters (A-Z)
  - Lowercase letters (a-z)  
  - Numbers (0-9)
  - Non-alphanumeric characters (!@#$%^&*()_+-=[]{}|;:,.<>?)

## Performance Optimization

### Configure Memory Settings
```bash
# Set maximum memory usage (example: 2GB = 2048 MB)
sudo /opt/mssql/bin/mssql-conf set memory.memorylimitmb 2048

# Restart SQL Server to apply changes
sudo systemctl restart mssql-server
```

### Configure Parallel Processing
```bash
# Set maximum degree of parallelism
sudo /opt/mssql/bin/mssql-conf set sqlagent.databasemailprofile default

# Restart service
sudo systemctl restart mssql-server
```

## Backup and Maintenance

### Enable SQL Server Agent
```bash
# Enable SQL Server Agent for jobs and maintenance
sudo /opt/mssql/bin/mssql-conf set sqlagent.enabled true

# Restart SQL Server
sudo systemctl restart mssql-server
```

### Basic Backup Script
```sql
-- Connect to SQL Server
sqlcmd -S localhost -U sa

-- Backup database
BACKUP DATABASE ERPDatabase 
TO DISK = '/var/opt/mssql/data/ERPDatabase.bak'
WITH FORMAT, INIT;
GO

quit
```

## Next Steps

After successful installation:

1. ✅ Install and configure SQL Server ✓
2. 📋 Create application databases using [Database-Setup.md](Database-Setup.md)
3. 🔧 Configure application connection strings
4. 🔒 Set up proper security and users
5. 📊 Configure backup and maintenance plans
6. 🚀 Deploy and test your ERP applications

## Useful Commands Reference

```bash
# Check SQL Server version
sqlcmd -S localhost -U sa -Q "SELECT @@VERSION"

# List all databases
sqlcmd -S localhost -U sa -Q "SELECT name FROM sys.databases"

# Check SQL Server configuration
sudo /opt/mssql/bin/mssql-conf list

# View SQL Server logs
sudo tail -f /var/opt/mssql/log/errorlog

# Check service status
sudo systemctl status mssql-server --no-pager
```

---

**Note**: Replace `[WSL_IP_ADDRESS]`, `[DATABASE_NAME]`, `[SA_PASSWORD]` with your actual values.

**Created**: $(date)  
**Version**: 1.0  
**Target Environment**: WSL 2 Ubuntu + SQL Server 2022