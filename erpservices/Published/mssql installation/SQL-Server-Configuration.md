# SQL Server Configuration Guide - Advanced Settings

## Overview
This guide covers advanced configuration options for SQL Server running on WSL, including performance optimization, security hardening, and operational settings for the ERP system.

## Basic Configuration

### Initial Setup Verification
```bash
# Check current SQL Server configuration
wsl sudo /opt/mssql/bin/mssql-conf list

# Verify service status
wsl sudo systemctl status mssql-server --no-pager

# Check SQL Server version and edition
wsl sqlcmd -S localhost -U sa -Q "SELECT @@VERSION, SERVERPROPERTY('Edition')"
```

### Configuration File Location
```bash
# Main configuration file
wsl sudo cat /var/opt/mssql/mssql.conf

# Backup current configuration
wsl sudo cp /var/opt/mssql/mssql.conf /var/opt/mssql/mssql.conf.backup
```

## Memory Configuration

### Set Memory Limits
```bash
# Set maximum memory (example: 4GB = 4096 MB)
wsl sudo /opt/mssql/bin/mssql-conf set memory.memorylimitmb 4096

# Set minimum memory (example: 1GB = 1024 MB)
wsl sudo /opt/mssql/bin/mssql-conf set memory.minserver 1024

# Apply changes
wsl sudo systemctl restart mssql-server
```

### Memory Configuration Guidelines
```bash
# For systems with 8GB RAM: Set limit to 6GB (6144 MB)
wsl sudo /opt/mssql/bin/mssql-conf set memory.memorylimitmb 6144

# For systems with 16GB RAM: Set limit to 12GB (12288 MB)
wsl sudo /opt/mssql/bin/mssql-conf set memory.memorylimitmb 12288

# For systems with 32GB RAM: Set limit to 24GB (24576 MB)
wsl sudo /opt/mssql/bin/mssql-conf set memory.memorylimitmb 24576
```

### Verify Memory Settings
```sql
-- Connect to SQL Server
wsl sqlcmd -S localhost -U sa

-- Check memory configuration
SELECT 
    name,
    value_in_use as current_value,
    minimum,
    maximum,
    is_dynamic
FROM sys.configurations 
WHERE name LIKE '%memory%';
GO

-- Check actual memory usage
SELECT 
    (physical_memory_in_use_kb/1024) AS Memory_MB,
    (locked_page_allocations_kb/1024) AS Locked_Pages_MB,
    (total_virtual_address_space_kb/1024) AS Virtual_Memory_MB,
    process_physical_memory_low,
    process_virtual_memory_low
FROM sys.dm_os_process_memory;
GO

quit
```

## Network Configuration

### Configure Network Settings
```bash
# Set TCP port (default is 1433)
wsl sudo /opt/mssql/bin/mssql-conf set network.tcpport 1433

# Enable/disable TCP protocol
wsl sudo /opt/mssql/bin/mssql-conf set network.tcpenabled true

# Set IP address to listen on (0.0.0.0 for all interfaces)
wsl sudo /opt/mssql/bin/mssql-conf set network.ipaddress 0.0.0.0

# Restart to apply changes
wsl sudo systemctl restart mssql-server
```

### Advanced Network Configuration
```bash
# Set maximum concurrent connections
wsl sudo /opt/mssql/bin/mssql-conf set sqlagent.maxworkerthreads 255

# Configure remote query timeout (seconds)
wsl sudo /opt/mssql/bin/mssql-conf set network.rpcTimeout 600

# Set network packet size
wsl sudo /opt/mssql/bin/mssql-conf set network.packetsize 4096
```

### Verify Network Configuration
```sql
-- Check network configuration
SELECT 
    name,
    value_in_use,
    description
FROM sys.configurations 
WHERE name IN (
    'remote access',
    'remote admin connections',
    'remote login timeout',
    'remote proc trans',
    'remote query timeout'
);
GO

-- Check listening ports and connections
SELECT 
    local_net_address,
    local_tcp_port,
    state_desc,
    connection_id
FROM sys.dm_exec_connections;
GO
```

## Security Configuration

### Authentication Mode
```sql
-- Check current authentication mode
SELECT SERVERPROPERTY('IsIntegratedSecurityOnly') as IsWindowsAuth;

-- SQL Server on Linux uses SQL Authentication by default
-- No change needed for mixed mode authentication
```

### Password Policies
```sql
-- Create logins with specific password policies
CREATE LOGIN ERPAdmin 
WITH PASSWORD = 'ERP_Adm1n_P@ssw0rd!',
CHECK_POLICY = ON,
CHECK_EXPIRATION = ON;

-- Create service accounts with no expiration
CREATE LOGIN ERPService 
WITH PASSWORD = 'ERP_S3rv1c3_P@ssw0rd!',
CHECK_POLICY = ON,
CHECK_EXPIRATION = OFF;
```

### Server-Level Security Settings
```sql
-- Configure server security options
-- Disable unnecessary features for security
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;

-- Disable xp_cmdshell (command shell access)
EXEC sp_configure 'xp_cmdshell', 0;
RECONFIGURE;

-- Disable Ole Automation Procedures
EXEC sp_configure 'Ole Automation Procedures', 0;
RECONFIGURE;

-- Configure remote access (disable if not needed)
EXEC sp_configure 'remote access', 0;
RECONFIGURE;

-- Hide advanced options again
EXEC sp_configure 'show advanced options', 0;
RECONFIGURE;
```

### Audit Configuration
```bash
# Enable SQL Server audit logging
wsl sudo /opt/mssql/bin/mssql-conf set sqlagent.enabled true

# Configure audit file location
wsl sudo mkdir -p /var/opt/mssql/audit
wsl sudo chown mssql:mssql /var/opt/mssql/audit
```

```sql
-- Create server audit
CREATE SERVER AUDIT ERP_Security_Audit
TO FILE (FILEPATH = '/var/opt/mssql/audit/')
WITH (QUEUE_DELAY = 1000, ON_FAILURE = CONTINUE);

-- Enable the audit
ALTER SERVER AUDIT ERP_Security_Audit WITH (STATE = ON);

-- Create audit specification for login events
CREATE SERVER AUDIT SPECIFICATION ERP_Login_Audit
FOR SERVER AUDIT ERP_Security_Audit
ADD (FAILED_LOGIN_GROUP),
ADD (SUCCESSFUL_LOGIN_GROUP),
ADD (LOGOUT_GROUP);

-- Enable the audit specification
ALTER SERVER AUDIT SPECIFICATION ERP_Login_Audit WITH (STATE = ON);
```

## Performance Configuration

### Parallelism Settings
```sql
-- Configure max degree of parallelism
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;

-- Set MAXDOP (0 = use all processors, recommended: number of cores)
EXEC sp_configure 'max degree of parallelism', 4;
RECONFIGURE;

-- Set cost threshold for parallelism
EXEC sp_configure 'cost threshold for parallelism', 50;
RECONFIGURE;

EXEC sp_configure 'show advanced options', 0;
RECONFIGURE;
```

### TempDB Configuration
```sql
-- Check current tempdb configuration
SELECT 
    name,
    physical_name,
    size * 8 / 1024 AS size_MB,
    growth,
    is_percent_growth
FROM sys.master_files 
WHERE database_id = 2;

-- Add additional tempdb data files (recommended: 1 per CPU core up to 8)
ALTER DATABASE tempdb
ADD FILE (
    NAME = 'tempdev2',
    FILENAME = '/var/opt/mssql/data/tempdb2.mdf',
    SIZE = 100MB,
    FILEGROWTH = 10MB
);

ALTER DATABASE tempdb
ADD FILE (
    NAME = 'tempdev3',
    FILENAME = '/var/opt/mssql/data/tempdb3.mdf',
    SIZE = 100MB,
    FILEGROWTH = 10MB
);
```

### Database File Configuration
```sql
-- Configure ERP database file settings
USE master;
GO

-- Modify data file growth settings
ALTER DATABASE ERPDatabase
MODIFY FILE (
    NAME = 'ERPDatabase_Data',
    FILEGROWTH = 100MB
);

-- Modify log file growth settings  
ALTER DATABASE ERPDatabase
MODIFY FILE (
    NAME = 'ERPDatabase_Log',
    FILEGROWTH = 50MB
);

-- Set initial file sizes appropriately
ALTER DATABASE ERPDatabase
MODIFY FILE (
    NAME = 'ERPDatabase_Data',
    SIZE = 500MB
);
```

## Backup Configuration

### Configure Backup Compression
```sql
-- Enable backup compression by default
EXEC sp_configure 'backup compression default', 1;
RECONFIGURE;
```

### Backup Device Configuration
```bash
# Create backup directory structure
wsl sudo mkdir -p /var/opt/mssql/backup/{full,diff,log}
wsl sudo chown -R mssql:mssql /var/opt/mssql/backup
wsl sudo chmod 755 /var/opt/mssql/backup
```

```sql
-- Create backup devices
EXEC sp_addumpdevice 'disk', 'ERP_Full_Backup',
'/var/opt/mssql/backup/full/ERP_Full.bak';

EXEC sp_addumpdevice 'disk', 'ERP_Diff_Backup', 
'/var/opt/mssql/backup/diff/ERP_Diff.bak';

EXEC sp_addumpdevice 'disk', 'ERP_Log_Backup',
'/var/opt/mssql/backup/log/ERP_Log.bak';
```

## Agent and Jobs Configuration

### Enable SQL Server Agent
```bash
# Enable SQL Server Agent
wsl sudo /opt/mssql/bin/mssql-conf set sqlagent.enabled true

# Restart SQL Server to apply changes
wsl sudo systemctl restart mssql-server

# Verify SQL Server Agent is running
wsl sudo systemctl status mssql-server --no-pager | grep -i agent
```

### Configure Agent Properties
```sql
-- Check SQL Server Agent configuration
SELECT 
    name,
    value_in_use
FROM sys.configurations 
WHERE name LIKE '%agent%';

-- Configure agent job history
EXEC msdb.dbo.sp_set_sqlagent_properties 
    @jobhistory_max_rows = 10000,
    @jobhistory_max_rows_per_job = 1000;
```

### Create Maintenance Jobs
```sql
-- Create backup maintenance job
USE msdb;
GO

EXEC dbo.sp_add_job
    @job_name = N'ERP Database Backup - Full';

EXEC dbo.sp_add_jobstep
    @job_name = N'ERP Database Backup - Full',
    @step_name = N'Backup Database',
    @subsystem = N'TSQL',
    @command = N'
BACKUP DATABASE ERPDatabase 
TO DISK = ''/var/opt/mssql/backup/full/ERPDatabase_Full.bak''
WITH COMPRESSION, CHECKSUM, INIT;',
    @retry_attempts = 3,
    @retry_interval = 5;

EXEC dbo.sp_add_schedule
    @schedule_name = N'Daily at 2 AM',
    @freq_type = 4,
    @freq_interval = 1,
    @active_start_time = 020000;

EXEC dbo.sp_attach_schedule
    @job_name = N'ERP Database Backup - Full',
    @schedule_name = N'Daily at 2 AM';

EXEC dbo.sp_add_jobserver
    @job_name = N'ERP Database Backup - Full';
```

## Monitoring Configuration

### Configure Performance Counters
```sql
-- Enable query store for performance monitoring
ALTER DATABASE ERPDatabase SET QUERY_STORE = ON;

ALTER DATABASE ERPDatabase SET QUERY_STORE (
    OPERATION_MODE = READ_WRITE,
    CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30),
    DATA_FLUSH_INTERVAL_SECONDS = 900,
    INTERVAL_LENGTH_MINUTES = 60,
    MAX_STORAGE_SIZE_MB = 1000,
    QUERY_CAPTURE_MODE = AUTO,
    SIZE_BASED_CLEANUP_MODE = AUTO
);
```

### Extended Events Configuration
```sql
-- Create extended events session for monitoring
CREATE EVENT SESSION [ERP_Performance_Monitor] ON SERVER 
ADD EVENT sqlserver.sql_batch_completed(
    ACTION(sqlserver.client_hostname,sqlserver.database_name,sqlserver.username)
    WHERE ([duration]>(5000000))), -- Queries longer than 5 seconds
ADD EVENT sqlserver.sql_statement_completed(
    ACTION(sqlserver.client_hostname,sqlserver.database_name,sqlserver.username)
    WHERE ([duration]>(5000000)))
ADD TARGET package0.ring_buffer(SET max_events_limit=(1000))
WITH (MAX_MEMORY=4096 KB,EVENT_RETENTION_MODE=ALLOW_SINGLE_EVENT_LOSS,
      MAX_DISPATCH_LATENCY=30 SECONDS,MAX_EVENT_SIZE=0 KB,
      MEMORY_PARTITION_MODE=NONE,TRACK_CAUSALITY=OFF,STARTUP_STATE=ON);

-- Start the session
ALTER EVENT SESSION [ERP_Performance_Monitor] ON SERVER STATE = START;
```

## Database-Specific Configuration

### Configure ERP Database Settings
```sql
-- Set database options for optimal performance
ALTER DATABASE ERPDatabase SET READ_COMMITTED_SNAPSHOT ON;
ALTER DATABASE ERPDatabase SET ALLOW_SNAPSHOT_ISOLATION ON;
ALTER DATABASE ERPDatabase SET AUTO_CREATE_STATISTICS ON;
ALTER DATABASE ERPDatabase SET AUTO_UPDATE_STATISTICS ON;
ALTER DATABASE ERPDatabase SET AUTO_UPDATE_STATISTICS_ASYNC ON;

-- Set appropriate recovery model
ALTER DATABASE ERPDatabase SET RECOVERY SIMPLE; -- or FULL for production

-- Configure database file settings
ALTER DATABASE ERPDatabase SET AUTO_SHRINK OFF;
ALTER DATABASE ERPDatabase SET AUTO_CLOSE OFF;
```

### Create Resource Governor Configuration
```sql
-- Enable Resource Governor
ALTER RESOURCE GOVERNOR RECONFIGURE;

-- Create resource pool for ERP applications
CREATE RESOURCE POOL ERP_Pool
WITH (
    MIN_CPU_PERCENT = 20,
    MAX_CPU_PERCENT = 80,
    MIN_MEMORY_PERCENT = 20,
    MAX_MEMORY_PERCENT = 80
);

-- Create workload group
CREATE WORKLOAD GROUP ERP_Group
USING ERP_Pool;

-- Create classifier function
CREATE FUNCTION dbo.ERP_Classifier()
RETURNS sysname
WITH SCHEMABINDING
AS
BEGIN
    DECLARE @WorkloadGroup sysname;
    
    IF (ORIGINAL_LOGIN() LIKE '%APIUser%')
        SET @WorkloadGroup = 'ERP_Group';
    ELSE
        SET @WorkloadGroup = 'default';
        
    RETURN @WorkloadGroup;
END;

-- Register classifier function
ALTER RESOURCE GOVERNOR WITH (CLASSIFIER_FUNCTION = dbo.ERP_Classifier);
ALTER RESOURCE GOVERNOR RECONFIGURE;
```

## Configuration Scripts

### Save Current Configuration Script
```bash
#!/bin/bash
# Save as save_config.sh

echo "Saving SQL Server Configuration..."
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/tmp/sql_config_backup_$DATE"

mkdir -p $BACKUP_DIR

# Save mssql.conf
wsl sudo cp /var/opt/mssql/mssql.conf $BACKUP_DIR/

# Save SQL Server configuration
wsl sqlcmd -S localhost -U sa -Q "
SELECT 
    name,
    value_in_use,
    minimum,
    maximum,
    is_dynamic,
    description
FROM sys.configurations
ORDER BY name;
" -o $BACKUP_DIR/sql_configurations.txt

# Save database configurations
wsl sqlcmd -S localhost -U sa -Q "
SELECT 
    name,
    recovery_model_desc,
    state_desc,
    user_access_desc,
    is_read_committed_snapshot_on,
    is_allow_snapshot_isolation_on,
    is_auto_create_stats_on,
    is_auto_update_stats_on
FROM sys.databases;
" -o $BACKUP_DIR/database_configs.txt

echo "Configuration saved to $BACKUP_DIR"
```

### Performance Optimization Script
```bash
#!/bin/bash
# Save as optimize_performance.sh

echo "Applying SQL Server Performance Optimizations..."

# Set memory limits (adjust based on your system)
wsl sudo /opt/mssql/bin/mssql-conf set memory.memorylimitmb 4096

# Configure network settings
wsl sudo /opt/mssql/bin/mssql-conf set network.packetsize 4096

# Enable agent
wsl sudo /opt/mssql/bin/mssql-conf set sqlagent.enabled true

# Restart SQL Server
wsl sudo systemctl restart mssql-server

# Wait for startup
sleep 10

# Apply SQL-level optimizations
wsl sqlcmd -S localhost -U sa -Q "
-- Configure parallelism
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
EXEC sp_configure 'max degree of parallelism', 4;
EXEC sp_configure 'cost threshold for parallelism', 50;
EXEC sp_configure 'backup compression default', 1;
RECONFIGURE;
EXEC sp_configure 'show advanced options', 0;
RECONFIGURE;

-- Configure database settings
ALTER DATABASE ERPDatabase SET READ_COMMITTED_SNAPSHOT ON;
ALTER DATABASE ERPDatabase SET AUTO_UPDATE_STATISTICS_ASYNC ON;

PRINT 'Performance optimizations applied successfully!';
"

echo "Performance optimization complete!"
```

## Monitoring and Maintenance

### Create Monitoring Views
```sql
-- Create view for connection monitoring
CREATE VIEW vw_ERP_Connections AS
SELECT 
    s.session_id,
    s.login_name,
    s.host_name,
    s.program_name,
    s.client_interface_name,
    s.login_time,
    s.last_request_start_time,
    s.status,
    c.connect_time,
    c.net_transport,
    c.client_net_address,
    DB_NAME(s.database_id) as database_name
FROM sys.dm_exec_sessions s
LEFT JOIN sys.dm_exec_connections c ON s.session_id = c.session_id
WHERE s.is_user_process = 1;
```

### Regular Maintenance Tasks
```sql
-- Create maintenance procedure
CREATE PROCEDURE sp_ERP_Maintenance
AS
BEGIN
    -- Update statistics
    EXEC sp_updatestats;
    
    -- Rebuild/reorganize indexes
    DECLARE @sql NVARCHAR(MAX) = '';
    SELECT @sql = @sql + 
        CASE 
            WHEN avg_fragmentation_in_percent > 30 
            THEN 'ALTER INDEX [' + i.name + '] ON [' + SCHEMA_NAME(t.schema_id) + '].[' + t.name + '] REBUILD;' + CHAR(13)
            WHEN avg_fragmentation_in_percent > 5 
            THEN 'ALTER INDEX [' + i.name + '] ON [' + SCHEMA_NAME(t.schema_id) + '].[' + t.name + '] REORGANIZE;' + CHAR(13)
            ELSE ''
        END
    FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ps
    INNER JOIN sys.indexes i ON ps.object_id = i.object_id AND ps.index_id = i.index_id
    INNER JOIN sys.tables t ON i.object_id = t.object_id
    WHERE avg_fragmentation_in_percent > 5;
    
    IF LEN(@sql) > 0
        EXEC sp_executesql @sql;
        
    PRINT 'Maintenance completed successfully';
END;
```

## Configuration Validation

### Validation Script
```sql
-- Validate configuration settings
SELECT 'Memory Configuration' as Check_Type,
    CASE 
        WHEN value_in_use > 2048 THEN 'OK'
        ELSE 'WARNING: Memory limit may be too low'
    END as Status
FROM sys.configurations WHERE name = 'max server memory (MB)'

UNION ALL

SELECT 'Backup Compression',
    CASE 
        WHEN value_in_use = 1 THEN 'OK'
        ELSE 'WARNING: Backup compression not enabled'
    END
FROM sys.configurations WHERE name = 'backup compression default'

UNION ALL

SELECT 'Max Degree of Parallelism',
    CASE 
        WHEN value_in_use BETWEEN 1 AND 8 THEN 'OK'
        ELSE 'WARNING: MAXDOP may need adjustment'
    END
FROM sys.configurations WHERE name = 'max degree of parallelism';
```

---

**Note**: Adjust all memory and performance settings based on your specific hardware configuration and workload requirements.

**Version**: 1.0  
**Target Environment**: WSL 2 Ubuntu + SQL Server 2022 + ERP System