# SQL Server on WSL - Troubleshooting Guide

## Overview
This guide provides solutions to common issues encountered when running SQL Server on Windows Subsystem for Linux (WSL) and connecting to it from Windows applications.

## Quick Diagnostic Commands

### System Status Check
```bash
# Check WSL version and status
wsl --version
wsl --list --verbose

# Check Ubuntu status
wsl --distribution Ubuntu --exec echo "WSL is running"

# Get WSL IP address
wsl hostname -I | awk '{print $1}'
```

### SQL Server Status Check
```bash
# Check SQL Server service status
wsl sudo systemctl status mssql-server --no-pager

# Check SQL Server processes
wsl ps aux | grep mssql

# Check listening ports
wsl sudo netstat -tuln | grep 1433

# Test local connection
wsl sqlcmd -S localhost -U sa -Q "SELECT @@SERVERNAME, @@VERSION"
```

## Common Issues and Solutions

### Issue 1: SQL Server Won't Start

#### Symptoms:
- Service fails to start
- Error: "Job for mssql-server.service failed"
- Cannot connect to SQL Server

#### Diagnostic Commands:
```bash
# Check service status
wsl sudo systemctl status mssql-server --no-pager

# View detailed error logs
wsl sudo journalctl -u mssql-server -f

# Check SQL Server error log
wsl sudo tail -f /var/opt/mssql/log/errorlog

# Check system resources
wsl free -h
wsl df -h
```

#### Solutions:

**1. Insufficient Memory**
```bash
# Check available memory
wsl free -h

# If less than 2GB available, configure SQL Server memory limit
wsl sudo /opt/mssql/bin/mssql-conf set memory.memorylimitmb 1024

# Restart SQL Server
wsl sudo systemctl restart mssql-server
```

**2. Disk Space Issues**
```bash
# Check disk usage
wsl df -h

# Clean up space if needed
wsl sudo apt-get autoremove
wsl sudo apt-get autoclean

# Check SQL Server data directory
wsl ls -la /var/opt/mssql/data/
```

**3. Configuration Issues**
```bash
# Reset SQL Server configuration
wsl sudo /opt/mssql/bin/mssql-conf setup

# Check configuration file
wsl sudo cat /var/opt/mssql/mssql.conf
```

**4. Permission Issues**
```bash
# Fix SQL Server file permissions
wsl sudo chown -R mssql:mssql /var/opt/mssql/
wsl sudo chmod -R 755 /var/opt/mssql/
```

### Issue 2: Cannot Connect from Windows

#### Symptoms:
- Connection timeout errors
- "Server not found or not accessible"
- Network-related errors

#### Diagnostic Steps:

**1. Check WSL Network Configuration**
```bash
# Get WSL IP address
wsl hostname -I

# Check network interface
wsl ip addr show

# Check routing
wsl ip route show
```

**2. Test Network Connectivity from Windows**
```powershell
# Test ping
ping 192.168.68.113

# Test port connectivity
Test-NetConnection -ComputerName 192.168.68.113 -Port 1433

# Alternative with telnet
telnet 192.168.68.113 1433
```

**3. Check SQL Server Network Configuration**
```bash
# Verify SQL Server is listening on all interfaces
wsl sudo netstat -tuln | grep 1433

# Check SQL Server network configuration
wsl sqlcmd -S localhost -U sa -Q "
SELECT 
    name,
    value_in_use
FROM sys.configurations 
WHERE name IN ('remote access', 'remote admin connections')
"
```

#### Solutions:

**1. WSL IP Address Changed**
```bash
# Get current IP
wsl hostname -I

# Update connection strings with new IP
# Use dynamic IP discovery in your applications
```

**2. Windows Firewall Blocking Connection**
```powershell
# Check firewall rules
Get-NetFirewallRule -DisplayName "*SQL*" | Select DisplayName, Enabled, Direction

# Create firewall rule if needed
New-NetFirewallRule -DisplayName "SQL Server WSL" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
```

**3. SQL Server Not Configured for Remote Connections**
```sql
-- Connect locally first
wsl sqlcmd -S localhost -U sa

-- Enable remote connections
EXEC sp_configure 'remote access', 1;
RECONFIGURE;
GO

-- Check TCP/IP is enabled
SELECT name, value_in_use FROM sys.configurations WHERE name = 'remote access';
GO
```

### Issue 3: Authentication Failures

#### Symptoms:
- "Login failed for user"
- "Cannot connect to server"
- "Invalid username or password"

#### Diagnostic Commands:
```sql
-- Check if login exists
SELECT name, is_disabled FROM sys.server_principals WHERE name = 'YourUsername';

-- Check database users
USE YourDatabase;
SELECT name, type_desc FROM sys.database_principals WHERE name = 'YourUsername';

-- Check login permissions
SELECT 
    p.state_desc,
    p.permission_name,
    s.name
FROM sys.server_permissions p
JOIN sys.server_principals s ON p.grantee_principal_id = s.principal_id
WHERE s.name = 'YourUsername';
```

#### Solutions:

**1. User Doesn't Exist**
```sql
-- Create login
CREATE LOGIN YourUsername WITH PASSWORD = 'YourStrongPassword';

-- Create database user
USE YourDatabase;
CREATE USER YourUsername FOR LOGIN YourUsername;

-- Assign roles
ALTER ROLE db_datareader ADD MEMBER YourUsername;
ALTER ROLE db_datawriter ADD MEMBER YourUsername;
```

**2. Password Policy Issues**
```sql
-- Check password policy
SELECT is_policy_checked FROM sys.sql_logins WHERE name = 'YourUsername';

-- Create login with password policy options
CREATE LOGIN YourUsername 
WITH PASSWORD = 'YourStrongPassword',
CHECK_POLICY = OFF,
CHECK_EXPIRATION = OFF;
```

**3. Account Locked or Disabled**
```sql
-- Check login status
SELECT 
    name,
    is_disabled,
    is_locked
FROM sys.server_principals 
WHERE name = 'YourUsername';

-- Enable account if disabled
ALTER LOGIN YourUsername ENABLE;
```

### Issue 4: Connection Timeouts

#### Symptoms:
- "Timeout expired" errors
- Slow query performance
- Connection pool exhaustion

#### Diagnostic Commands:
```sql
-- Check active connections
SELECT 
    DB_NAME(database_id) as DatabaseName,
    COUNT(*) as ConnectionCount,
    login_name,
    host_name,
    program_name
FROM sys.dm_exec_sessions 
WHERE database_id > 0 
GROUP BY database_id, login_name, host_name, program_name
ORDER BY ConnectionCount DESC;

-- Check blocking processes
SELECT 
    blocking_session_id,
    session_id,
    wait_type,
    wait_time,
    wait_resource
FROM sys.dm_exec_requests 
WHERE blocking_session_id > 0;

-- Check long-running queries
SELECT 
    session_id,
    start_time,
    status,
    command,
    cpu_time,
    total_elapsed_time,
    text
FROM sys.dm_exec_requests r
CROSS APPLY sys.dm_exec_sql_text(r.sql_handle)
WHERE total_elapsed_time > 30000; -- 30 seconds
```

#### Solutions:

**1. Increase Connection Timeout**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=sa;Password=YourPassword;TrustServerCertificate=true;Connection Timeout=60;Command Timeout=120;"
  }
}
```

**2. Optimize Connection Pool**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=sa;Password=YourPassword;TrustServerCertificate=true;Min Pool Size=10;Max Pool Size=100;Pooling=true;"
  }
}
```

**3. SQL Server Performance Tuning**
```bash
# Increase SQL Server memory
wsl sudo /opt/mssql/bin/mssql-conf set memory.memorylimitmb 4096

# Configure max worker threads
wsl sudo /opt/mssql/bin/mssql-conf set sqlagent.workerthreads 255

# Restart SQL Server
wsl sudo systemctl restart mssql-server
```

### Issue 5: Database Not Found

#### Symptoms:
- "Cannot open database"
- "Database does not exist"
- "Invalid database name"

#### Diagnostic Commands:
```sql
-- List all databases
SELECT name FROM sys.databases;

-- Check database status
SELECT 
    name,
    state_desc,
    user_access_desc,
    is_read_only
FROM sys.databases;

-- Check if database files exist
SELECT 
    name,
    physical_name,
    state_desc
FROM sys.master_files;
```

#### Solutions:

**1. Create Missing Database**
```sql
-- Create database
CREATE DATABASE ERPDatabase;

-- Verify creation
SELECT name FROM sys.databases WHERE name = 'ERPDatabase';
```

**2. Restore Database from Backup**
```sql
-- Check backup file
RESTORE HEADERONLY FROM DISK = '/var/opt/mssql/data/database.bak';

-- Restore database
RESTORE DATABASE ERPDatabase 
FROM DISK = '/var/opt/mssql/data/database.bak'
WITH REPLACE;
```

**3. Database is Offline**
```sql
-- Check database status
SELECT name, state_desc FROM sys.databases WHERE name = 'ERPDatabase';

-- Bring database online
ALTER DATABASE ERPDatabase SET ONLINE;
```

### Issue 6: Performance Issues

#### Symptoms:
- Slow query execution
- High CPU usage
- Memory pressure
- Disk I/O bottlenecks

#### Diagnostic Commands:
```bash
# Check system resources
wsl top -p $(wsl pgrep sqlservr)
wsl free -h
wsl df -h
wsl iostat 1 5

# Check SQL Server performance
wsl sqlcmd -S localhost -U sa -Q "
SELECT 
    counter_name,
    instance_name,
    cntr_value
FROM sys.dm_os_performance_counters
WHERE object_name LIKE '%Memory Manager%'
   OR object_name LIKE '%Buffer Manager%'
   OR object_name LIKE '%SQL Statistics%'
ORDER BY object_name, counter_name;
"
```

#### Solutions:

**1. Memory Optimization**
```bash
# Set appropriate memory limits
wsl sudo /opt/mssql/bin/mssql-conf set memory.memorylimitmb 2048

# Check current memory usage
wsl sqlcmd -S localhost -U sa -Q "
SELECT 
    (physical_memory_in_use_kb/1024) AS Memory_usedby_Sqlserver_MB,
    (locked_page_allocations_kb/1024) AS Locked_pages_used_Sqlserver_MB,
    (total_virtual_address_space_kb/1024) AS Total_VAS_in_MB,
    process_physical_memory_low,
    process_virtual_memory_low
FROM sys.dm_os_process_memory;
"
```

**2. Index Optimization**
```sql
-- Find missing indexes
SELECT 
    migs.avg_total_user_cost * (migs.avg_user_impact / 100.0) * (migs.user_seeks + migs.user_scans) AS improvement_measure,
    'CREATE INDEX [missing_index_' + CONVERT(varchar, mig.index_group_handle) + '_' + CONVERT(varchar, mid.index_handle) + '_' + LEFT (PARSENAME(mid.statement, 1), 32) + ']'
    + ' ON ' + mid.statement
    + ' (' + ISNULL (mid.equality_columns,'')
    + CASE WHEN mid.equality_columns IS NOT NULL AND mid.inequality_columns IS NOT NULL THEN ',' ELSE '' END
    + ISNULL (mid.inequality_columns, '')
    + ')'
    + ISNULL (' INCLUDE (' + mid.included_columns + ')', '') AS create_index_statement,
    migs.*, mid.database_id, mid.[object_id]
FROM sys.dm_db_missing_index_groups mig
INNER JOIN sys.dm_db_missing_index_group_stats migs ON migs.group_handle = mig.index_group_handle
INNER JOIN sys.dm_db_missing_index_details mid ON mig.index_handle = mid.index_handle
WHERE migs.avg_total_user_cost * (migs.avg_user_impact / 100.0) * (migs.user_seeks + migs.user_scans) > 10
ORDER BY migs.avg_total_user_cost * migs.avg_user_impact * (migs.user_seeks + migs.user_scans) DESC;
```

**3. Query Optimization**
```sql
-- Find expensive queries
SELECT TOP 10
    st.text,
    cp.size_in_bytes,
    qs.execution_count,
    qs.total_worker_time,
    qs.total_worker_time / qs.execution_count AS avg_cpu_time,
    qs.total_elapsed_time,
    qs.total_elapsed_time / qs.execution_count AS avg_elapsed_time
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st
CROSS APPLY sys.dm_exec_cached_plans(qs.plan_handle) cp
ORDER BY qs.total_worker_time DESC;
```

## Advanced Troubleshooting

### Enable SQL Server Logging

```bash
# Enable detailed logging
wsl sudo /opt/mssql/bin/mssql-conf set filelocation.defaultlogdir /var/opt/mssql/log

# Set trace flags for troubleshooting
wsl sqlcmd -S localhost -U sa -Q "DBCC TRACEON(1204, 1222, -1)"

# Check active trace flags
wsl sqlcmd -S localhost -U sa -Q "DBCC TRACESTATUS(-1)"
```

### Monitor SQL Server Activity

```bash
# Create monitoring script
cat > /tmp/monitor_sql.sh << 'EOF'
#!/bin/bash
while true; do
    echo "=== $(date) ==="
    echo "CPU Usage:"
    wsl top -bn1 | grep sqlservr | awk '{print $9"%"}'
    
    echo "Memory Usage:"
    wsl free -m | grep Mem | awk '{print ($3/$2)*100"%"}'
    
    echo "Active Connections:"
    wsl sqlcmd -S localhost -U sa -Q "SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE is_user_process = 1" -h -1
    
    echo "Blocking Sessions:"
    wsl sqlcmd -S localhost -U sa -Q "SELECT COUNT(*) FROM sys.dm_exec_requests WHERE blocking_session_id > 0" -h -1
    
    echo "---"
    sleep 60
done
EOF

# Make executable
chmod +x /tmp/monitor_sql.sh

# Run monitoring
/tmp/monitor_sql.sh
```

### Backup and Recovery Issues

#### Backup Failures
```bash
# Check backup directory permissions
wsl ls -la /var/opt/mssql/backup/

# Create backup directory if missing
wsl sudo mkdir -p /var/opt/mssql/backup
wsl sudo chown mssql:mssql /var/opt/mssql/backup
```

#### Recovery Issues
```sql
-- Check database recovery model
SELECT name, recovery_model_desc FROM sys.databases;

-- Change recovery model if needed
ALTER DATABASE ERPDatabase SET RECOVERY SIMPLE;

-- Check for active transactions
SELECT * FROM sys.dm_tran_active_transactions;
```

## Automated Diagnostics Script

Create a comprehensive diagnostic script:

```bash
#!/bin/bash
# Save as diagnose_sql.sh

echo "===================================="
echo "SQL Server on WSL Diagnostic Report"
echo "===================================="
echo "Date: $(date)"
echo "===================================="

echo ""
echo "1. SYSTEM STATUS"
echo "=================="
echo "WSL Version:"
wsl --version | head -1

echo ""
echo "Ubuntu Status:"
wsl --distribution Ubuntu --exec echo "✅ WSL Ubuntu is accessible"

echo ""
echo "WSL IP Address:"
wsl hostname -I | awk '{print $1}'

echo ""
echo "System Resources:"
wsl free -h
echo ""
wsl df -h /

echo ""
echo "2. SQL SERVER STATUS"
echo "==================="
echo "Service Status:"
wsl sudo systemctl status mssql-server --no-pager | grep "Active:"

echo ""
echo "SQL Server Process:"
wsl ps aux | grep sqlservr | grep -v grep || echo "❌ SQL Server process not found"

echo ""
echo "Network Ports:"
wsl sudo netstat -tuln | grep 1433 || echo "❌ SQL Server not listening on port 1433"

echo ""
echo "3. CONNECTIVITY TESTS"
echo "===================="
echo "Local Connection Test:"
if wsl sqlcmd -S localhost -U sa -Q "SELECT @@SERVERNAME" > /dev/null 2>&1; then
    echo "✅ Local connection successful"
    echo "SQL Server Version:"
    wsl sqlcmd -S localhost -U sa -Q "SELECT @@VERSION" -h -1
else
    echo "❌ Local connection failed"
fi

echo ""
echo "Network Connectivity from Windows:"
WSL_IP=$(wsl hostname -I | awk '{print $1}')
if ping -n 1 $WSL_IP > /dev/null 2>&1; then
    echo "✅ Network ping successful to $WSL_IP"
else
    echo "❌ Network ping failed to $WSL_IP"
fi

echo ""
echo "4. DATABASE STATUS"
echo "=================="
if wsl sqlcmd -S localhost -U sa -Q "SELECT name FROM sys.databases" -h -1 > /dev/null 2>&1; then
    echo "Databases:"
    wsl sqlcmd -S localhost -U sa -Q "SELECT name FROM sys.databases" -h -1
else
    echo "❌ Cannot retrieve database list"
fi

echo ""
echo "5. PERFORMANCE METRICS"
echo "====================="
echo "Active Connections:"
wsl sqlcmd -S localhost -U sa -Q "SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE is_user_process = 1" -h -1 2>/dev/null || echo "❌ Cannot retrieve connection count"

echo ""
echo "Blocking Sessions:"
wsl sqlcmd -S localhost -U sa -Q "SELECT COUNT(*) FROM sys.dm_exec_requests WHERE blocking_session_id > 0" -h -1 2>/dev/null || echo "❌ Cannot retrieve blocking sessions"

echo ""
echo "Memory Usage:"
wsl sqlcmd -S localhost -U sa -Q "SELECT (physical_memory_in_use_kb/1024) AS Memory_MB FROM sys.dm_os_process_memory" -h -1 2>/dev/null || echo "❌ Cannot retrieve memory usage"

echo ""
echo "6. ERROR LOGS"
echo "============="
echo "Recent SQL Server Errors:"
wsl sudo tail -n 20 /var/opt/mssql/log/errorlog 2>/dev/null || echo "❌ Cannot access error log"

echo ""
echo "Recent System Journal Errors:"
wsl sudo journalctl -u mssql-server --no-pager -n 10 2>/dev/null || echo "❌ Cannot access system journal"

echo ""
echo "===================================="
echo "Diagnostic Complete"
echo "===================================="
```

Make it executable and run:
```bash
chmod +x diagnose_sql.sh
./diagnose_sql.sh > sql_diagnostic_report.txt
```

## Getting Help

### Log Collection for Support

```bash
# Collect all relevant logs
mkdir -p /tmp/sql_support_logs
wsl sudo cp /var/opt/mssql/log/errorlog* /tmp/sql_support_logs/
wsl sudo journalctl -u mssql-server > /tmp/sql_support_logs/systemd.log
wsl dmesg > /tmp/sql_support_logs/dmesg.log
wsl sudo /opt/mssql/bin/mssql-conf list > /tmp/sql_support_logs/mssql_config.txt

# Create archive
tar -czf sql_support_logs.tar.gz /tmp/sql_support_logs/
```

### Useful Resources

- [Microsoft SQL Server on Linux Documentation](https://docs.microsoft.com/en-us/sql/linux/)
- [WSL Documentation](https://docs.microsoft.com/en-us/windows/wsl/)
- [SQL Server Error Log Messages](https://docs.microsoft.com/en-us/sql/relational-databases/errors-events/)
- [SQL Server Performance Monitoring](https://docs.microsoft.com/en-us/sql/relational-databases/performance/)

### Community Support

- Stack Overflow: [sql-server] + [windows-subsystem-for-linux] tags
- Microsoft Q&A: SQL Server on Linux section
- Reddit: r/SQLServer
- DBA Stack Exchange

---

**Note**: Always ensure you have proper backups before making configuration changes.

**Version**: 1.0  
**Last Updated**: $(date)  
**Target Environment**: WSL 2 + Ubuntu + SQL Server 2022