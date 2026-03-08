# Windows to WSL SQL Server Connectivity Guide

## Overview
This guide explains how to establish and troubleshoot connections between Windows applications and SQL Server running in WSL (Windows Subsystem for Linux).

## Network Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Windows Host                         │
│  ┌─────────────────┐    Network Bridge    ┌─────────────────┐ │
│  │  Windows Apps   │ ◄────────────────────► │   WSL Ubuntu    │ │
│  │  - .NET APIs    │    192.168.68.x       │  - SQL Server   │ │
│  │  - SSMS         │                       │  - Port 1433    │ │
│  │  - Web Apps     │                       │                 │ │
│  └─────────────────┘                       └─────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## Connection Methods

### Method 1: Direct IP Connection (Recommended)

#### Get WSL IP Address
```bash
# From WSL Ubuntu
hostname -I | awk '{print $1}'
# Output example: 192.168.68.113
```

#### Connection String Format
```csharp
Server=192.168.68.113;Database=ERPDatabase;User Id=sa;Password=YourPassword;TrustServerCertificate=true;
```

### Method 2: Localhost Forwarding (Limited)

Some services automatically forward to localhost:
```csharp
Server=localhost;Database=ERPDatabase;User Id=sa;Password=YourPassword;TrustServerCertificate=true;
```

**Note**: This may not work consistently with WSL 2.

## ERP Application Configuration

### API Services Connection Strings

Update each API service's `appsettings.json`:

#### 1. Accounts.API
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=AccountsAPIUser;Password=Acc0unts_P@ss123;TrustServerCertificate=true;Timeout=30;"
  }
}
```

#### 2. Security.API
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=SecurityAPIUser;Password=S3cur1ty_P@ss123;TrustServerCertificate=true;Timeout=30;"
  }
}
```

#### 3. HRMS.API
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=HRMSAPIUser;Password=HRMS_P@ss123;TrustServerCertificate=true;Timeout=30;"
  }
}
```

#### 4. SCM.API
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=SCMAPIUser;Password=SCM_P@ss123;TrustServerCertificate=true;Timeout=30;"
  }
}
```

#### 5. Approval.API
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=ApprovalAPIUser;Password=Appr0val_P@ss123;TrustServerCertificate=true;Timeout=30;"
  }
}
```

#### 6. Mail.API
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=MailAPIUser;Password=M@il_P@ss123;TrustServerCertificate=true;Timeout=30;"
  }
}
```

### Connection String Parameters Explained

| Parameter | Purpose | Example Value |
|-----------|---------|---------------|
| `Server` | WSL SQL Server IP | `192.168.68.113` |
| `Database` | Target database name | `ERPDatabase` |
| `User Id` | SQL Server username | `AccountsAPIUser` |
| `Password` | User password | `Acc0unts_P@ss123` |
| `TrustServerCertificate` | Skip certificate validation | `true` |
| `Timeout` | Connection timeout (seconds) | `30` |
| `MultipleActiveResultSets` | Enable MARS | `true` (if needed) |
| `Encrypt` | Force encryption | `false` (for local dev) |

## Testing Connectivity

### 1. Network Connectivity Test

#### From Windows PowerShell:
```powershell
# Test network connectivity
Test-NetConnection -ComputerName 192.168.68.113 -Port 1433

# Alternative using telnet
telnet 192.168.68.113 1433
```

#### Expected Output:
```
ComputerName     : 192.168.68.113
RemoteAddress    : 192.168.68.113
RemotePort       : 1433
InterfaceAlias   : vEthernet (WSL)
SourceAddress    : 192.168.68.112
TcpTestSucceeded : True
```

### 2. SQL Server Connectivity Test

#### Using sqlcmd from Windows:
```cmd
# Install sqlcmd on Windows first if not available
# Test SA connection
sqlcmd -S 192.168.68.113 -U sa -P "YourSAPassword"

# Test application user
sqlcmd -S 192.168.68.113 -U AccountsAPIUser -P "Acc0unts_P@ss123"
```

#### Quick query test:
```cmd
sqlcmd -S 192.168.68.113 -U sa -Q "SELECT @@VERSION"
```

### 3. .NET Application Test

Create a simple console application to test connectivity:

```csharp
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string connectionString = "Server=192.168.68.113;Database=ERPDatabase;User Id=sa;Password=YourSAPassword;TrustServerCertificate=true;";
        
        await TestConnection(connectionString);
        await TestQueries(connectionString);
    }
    
    static async Task TestConnection(string connectionString)
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                Console.WriteLine("✅ Connection successful!");
                Console.WriteLine($"Server: {connection.DataSource}");
                Console.WriteLine($"Database: {connection.Database}");
                Console.WriteLine($"Server Version: {connection.ServerVersion}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Connection failed: {ex.Message}");
        }
    }
    
    static async Task TestQueries(string connectionString)
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                
                // Test basic query
                var command = new SqlCommand("SELECT @@SERVERNAME, @@VERSION", connection);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        Console.WriteLine($"Server Name: {reader[0]}");
                        Console.WriteLine($"Version: {reader[1]}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Query failed: {ex.Message}");
        }
    }
}
```

## Connection Pooling Configuration

### Entity Framework Core Configuration

#### In Startup.cs or Program.cs:
```csharp
services.AddDbContext<ERPDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(30);
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });
});
```

#### Connection Pool Settings:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=AccountsAPIUser;Password=Acc0unts_P@ss123;TrustServerCertificate=true;Min Pool Size=5;Max Pool Size=100;Connection Timeout=30;"
  }
}
```

## Security Configuration

### SSL/TLS Configuration (Production)

For production environments, configure SSL:

#### 1. Generate SSL Certificate in WSL:
```bash
# Create certificate directory
sudo mkdir -p /var/opt/mssql/ssl

# Generate private key
sudo openssl genrsa -out /var/opt/mssql/ssl/mssql.key 2048

# Generate certificate
sudo openssl req -new -key /var/opt/mssql/ssl/mssql.key -out /var/opt/mssql/ssl/mssql.csr

# Create self-signed certificate
sudo openssl x509 -req -days 365 -in /var/opt/mssql/ssl/mssql.csr -signkey /var/opt/mssql/ssl/mssql.key -out /var/opt/mssql/ssl/mssql.crt

# Set permissions
sudo chown mssql:mssql /var/opt/mssql/ssl/*
sudo chmod 600 /var/opt/mssql/ssl/*
```

#### 2. Configure SQL Server for SSL:
```bash
# Configure SSL
sudo /opt/mssql/bin/mssql-conf set network.tlscert /var/opt/mssql/ssl/mssql.crt
sudo /opt/mssql/bin/mssql-conf set network.tlskey /var/opt/mssql/ssl/mssql.key
sudo /opt/mssql/bin/mssql-conf set network.tlsprotocols 1.2

# Restart SQL Server
sudo systemctl restart mssql-server
```

#### 3. Updated Connection String for SSL:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=AccountsAPIUser;Password=Acc0unts_P@ss123;Encrypt=true;TrustServerCertificate=false;"
  }
}
```

## Performance Optimization

### Connection String Optimizations

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=AccountsAPIUser;Password=Acc0unts_P@ss123;TrustServerCertificate=true;Connection Timeout=30;Command Timeout=30;Min Pool Size=10;Max Pool Size=200;Pooling=true;MultipleActiveResultSets=true;"
  }
}
```

### SQL Server Configuration for Performance

#### Memory Configuration:
```bash
# Set SQL Server memory limit (example: 4GB)
sudo /opt/mssql/bin/mssql-conf set memory.memorylimitmb 4096

# Set max worker threads
sudo /opt/mssql/bin/mssql-conf set sqlagent.workerthreads 255

# Restart SQL Server
sudo systemctl restart mssql-server
```

## Monitoring and Logging

### Connection Monitoring Script

Save as `monitor_connections.sh`:

```bash
#!/bin/bash

echo "=== SQL Server Connection Monitor ==="
echo "Date: $(date)"
echo ""

# Check SQL Server status
echo "1. SQL Server Status:"
sudo systemctl status mssql-server --no-pager | grep "Active:"
echo ""

# Check listening ports
echo "2. Network Ports:"
sudo netstat -tuln | grep 1433
echo ""

# Check active connections
echo "3. Active Connections:"
sqlcmd -S localhost -U sa -Q "
SELECT 
    DB_NAME(database_id) as DatabaseName,
    COUNT(*) as ConnectionCount,
    login_name,
    host_name
FROM sys.dm_exec_sessions 
WHERE database_id > 0 
GROUP BY database_id, login_name, host_name
ORDER BY ConnectionCount DESC
" -h -1
echo ""

# Check WSL IP
echo "4. WSL IP Address:"
hostname -I | awk '{print $1}'
```

Make executable:
```bash
chmod +x monitor_connections.sh
```

### Application Logging

#### Configure logging in your .NET applications:

```csharp
// In Startup.cs or Program.cs
services.AddLogging(builder =>
{
    builder.AddConfiguration(configuration.GetSection("Logging"));
    builder.AddConsole();
    builder.AddEventSourceLogger();
});

// Add SQL Server logging
services.AddDbContext<ERPDbContext>(options =>
{
    options.UseSqlServer(connectionString)
           .LogTo(Console.WriteLine, LogLevel.Information)
           .EnableSensitiveDataLogging(); // Only for development
});
```

## Troubleshooting Common Issues

### Issue 1: Connection Timeout

**Symptoms:**
- "Timeout expired" errors
- Long delays connecting to database

**Solutions:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=AccountsAPIUser;Password=Acc0unts_P@ss123;TrustServerCertificate=true;Connection Timeout=60;Command Timeout=120;"
  }
}
```

### Issue 2: Cannot Connect to Server

**Diagnostic Steps:**
```bash
# 1. Check WSL IP hasn't changed
hostname -I

# 2. Check SQL Server is running
sudo systemctl status mssql-server --no-pager

# 3. Check port is listening
sudo netstat -tuln | grep 1433

# 4. Test from WSL locally
sqlcmd -S localhost -U sa -Q "SELECT @@SERVERNAME"
```

**From Windows:**
```powershell
# Test network connectivity
Test-NetConnection -ComputerName 192.168.68.113 -Port 1433

# Check Windows Firewall
Get-NetFirewallRule -DisplayName "*SQL*" | Select DisplayName, Enabled, Direction
```

### Issue 3: Authentication Failed

**Check user exists:**
```sql
-- Connect as SA
sqlcmd -S localhost -U sa

-- Check if login exists
SELECT name, is_disabled FROM sys.server_principals WHERE name = 'AccountsAPIUser';

-- Check database user
USE ERPDatabase;
SELECT name FROM sys.database_principals WHERE name = 'AccountsAPIUser';
```

### Issue 4: WSL IP Address Changed

WSL IP addresses can change after Windows restart.

#### Get current IP:
```bash
wsl hostname -I
```

#### Update connection strings automatically:
```csharp
// In your application startup
public static string GetWSLConnectionString(string template)
{
    try
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "wsl",
                Arguments = "hostname -I",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        
        string wslIP = output.Trim().Split(' ')[0];
        return template.Replace("{{WSL_IP}}", wslIP);
    }
    catch
    {
        return template.Replace("{{WSL_IP}}", "localhost");
    }
}

// Usage:
string connectionTemplate = "Server={{WSL_IP}};Database=ERPDatabase;User Id=sa;Password=YourPassword;TrustServerCertificate=true;";
string connectionString = GetWSLConnectionString(connectionTemplate);
```

## API Gateway Configuration

### Configure Ocelot for Database Connections

If using API Gateway, ensure it can reach the database:

```json
{
  "GlobalConfiguration": {
    "BaseUrl": "https://localhost:5000"
  },
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5001
        }
      ],
      "UpstreamPathTemplate": "/accounts/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ]
    }
  ]
}
```

### Database Health Checks

Add health checks for database connectivity:

```csharp
// In Startup.cs
services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "sql-server-check")
    .AddCheck("wsl-connectivity", () =>
    {
        // Custom check for WSL connectivity
        try
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    });

// Add health check endpoint
app.MapHealthChecks("/health");
```

## Best Practices

### 1. Connection Management
- Use connection pooling
- Dispose connections properly
- Implement retry logic
- Set appropriate timeouts

### 2. Security
- Use dedicated database users for each service
- Avoid using SA account in production
- Implement proper password policies
- Enable SQL Server audit logging

### 3. Monitoring
- Monitor connection counts
- Track query performance
- Log connection failures
- Set up alerts for connectivity issues

### 4. High Availability
- Plan for WSL IP changes
- Implement connection retry patterns
- Consider SQL Server clustering for production
- Regular backup and recovery testing

---

**Note**: Always replace example IP addresses, passwords, and database names with your actual values.

**Version**: 1.0  
**Target**: ERP System Connectivity