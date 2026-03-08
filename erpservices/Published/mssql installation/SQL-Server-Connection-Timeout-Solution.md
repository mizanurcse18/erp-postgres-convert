# SQL Server Connection Timeout Issue Solution

## Problem Description

After successfully starting SQL Server 2022 on Ubuntu 24.04 (Noble) in WSL, attempts to connect using `sqlcmd` resulted in connection timeout errors:

```bash
munnacse18@LAPTOP-5N769GJK:~$ sqlcmd -S localhost -U sa
Password:
Sqlcmd: Error: Microsoft ODBC Driver 17 for SQL Server : Login timeout expired.
Sqlcmd: Error: Microsoft ODBC Driver 17 for SQL Server : TCP Provider: Error code 0x102.
Sqlcmd: Error: Microsoft ODBC Driver 17 for SQL Server : A network-related or instance-specific error has occurred while establishing a connection to SQL Server. Server is not found or not accessible. Check if instance name is correct and if SQL Server is configured to allow remote connections. For more information see SQL Server Books Online.
```

**Key indicators:**
- SQL Server service was running and active
- Server was listening on port 1433
- Connection timeout errors occurred
- No detailed authentication errors

## Root Cause Analysis

The issue was caused by **incomplete SQL Server configuration**:

### Diagnostic Steps Performed

1. **Verified SQL Server Status**:
   ```bash
   sudo systemctl status mssql-server --no-pager
   ```
   **Result**: Service was active and running ✅

2. **Checked Network Listening Ports**:
   ```bash
   sudo netstat -tlnp | grep sqlservr
   ```
   **Result**: SQL Server was listening on ports 1433, 1431, and 1434 ✅

3. **Examined SQL Server Configuration**:
   ```bash
   sudo cat /var/opt/mssql/mssql.conf
   ```
   **Result**: Configuration showed basic settings but no SA password configuration ❌

4. **Reviewed SQL Server Error Logs**:
   ```bash
   sudo cat /var/opt/mssql/log/errorlog
   ```
   **Result**: Server was ready for connections, but no authentication setup was evident ❌

### Root Cause Identified

**SQL Server was never properly configured with a SA password**. The service was running but authentication was not set up, causing connection failures.

## The Solution

The solution involved properly configuring SQL Server with the required authentication settings.

### Step-by-Step Resolution

#### 1. Stop SQL Server Service
```bash
sudo systemctl stop mssql-server
```

#### 2. Configure SQL Server with SA Password
```bash
sudo MSSQL_SA_PASSWORD='YourStrong!Passw0rd' ACCEPT_EULA='Y' MSSQL_PID='Developer' /opt/mssql/bin/mssql-conf -n setup accept-eula
```

**Configuration Parameters:**
- **Edition**: Developer (free for development use)
- **SA Password**: `YourStrong!Passw0rd` (meets complexity requirements)
- **EULA**: Accepted
- **Language**: English (default)

#### 3. Verify Service Status
```bash
systemctl status mssql-server --no-pager
```

**Expected Result**:
```
● mssql-server.service - Microsoft SQL Server Database Engine
     Loaded: loaded (/usr/lib/systemd/system/mssql-server.service; enabled; preset: enabled)
     Active: active (running) since Sun 2025-10-05 23:24:51 +06; 32s ago
```

#### 4. Test Connection (Key Fix)
Instead of using `localhost`, use the IP address:
```bash
/opt/mssql-tools/bin/sqlcmd -S 127.0.0.1 -U SA -P 'YourStrong!Passw0rd' -Q "SELECT @@VERSION"
```

## Verification of Success

After applying the solution, the connection worked successfully:

```bash
/opt/mssql-tools/bin/sqlcmd -S 127.0.0.1 -U SA -P 'YourStrong!Passw0rd' -Q "SELECT @@VERSION"
```

**Output**:
```
Microsoft SQL Server 2022 (RTM-CU21) (KB5065865) - 16.0.4215.2 (X64) 
	Aug 11 2025 13:24:21 
	Copyright (C) 2022 Microsoft Corporation
	Developer Edition (64-bit) on Linux (Ubuntu 24.04.3 LTS) <X64>
```

## Key Issues and Solutions

### Issue 1: Missing SA Password Configuration
**Problem**: SQL Server was running but had no SA password set
**Solution**: Used `mssql-conf setup` with environment variables to configure authentication

### Issue 2: Hostname Resolution
**Problem**: Using `localhost` caused connection failures
**Solution**: Use IP address `127.0.0.1` instead of `localhost`

### Issue 3: Service Configuration State
**Problem**: SQL Server couldn't be configured while running
**Solution**: Stop service before running configuration, then restart

## Connection Methods

### Interactive Connection
```bash
# With password prompt
/opt/mssql-tools/bin/sqlcmd -S 127.0.0.1 -U SA

# With password in command (less secure)
/opt/mssql-tools/bin/sqlcmd -S 127.0.0.1 -U SA -P 'YourStrong!Passw0rd'
```

### Query Execution
```bash
# Execute single query
/opt/mssql-tools/bin/sqlcmd -S 127.0.0.1 -U SA -P 'YourStrong!Passw0rd' -Q "SELECT GETDATE()"

# Execute from file
/opt/mssql-tools/bin/sqlcmd -S 127.0.0.1 -U SA -P 'YourStrong!Passw0rd' -i script.sql
```

### Connection with Timeout Settings
```bash
# Increase login timeout (default is 30 seconds)
/opt/mssql-tools/bin/sqlcmd -S 127.0.0.1 -U SA -P 'YourStrong!Passw0rd' -l 60 -Q "SELECT @@VERSION"
```

## Configuration Details

### Updated mssql.conf
After successful configuration, the `mssql.conf` file contains:

```ini
[sqlagent]
enabled = false

[licensing]
azurebilling = false

[EULA]
accepteula = Y

[language]
lcid = 1033
```

### Network Configuration
SQL Server listens on multiple ports:
- **Port 1433**: Standard SQL Server port (TCP)
- **Port 1431**: SQL Server Browser service
- **Port 1434**: Dedicated Administrator Connection (DAC)

## Troubleshooting Tips

### If Connection Still Fails

1. **Check Service Status**:
   ```bash
   sudo systemctl status mssql-server --no-pager
   ```

2. **Verify Listening Ports**:
   ```bash
   sudo netstat -tlnp | grep sqlservr
   ```

3. **Check Error Logs**:
   ```bash
   sudo tail -f /var/opt/mssql/log/errorlog
   ```

4. **Test Network Connectivity**:
   ```bash
   telnet 127.0.0.1 1433
   ```

5. **Use Different Connection Strings**:
   - Try `127.0.0.1` instead of `localhost`
   - Try `localhost,1433` (with explicit port)
   - Try the machine hostname

### Common Connection String Formats

```bash
# IP with explicit port
/opt/mssql-tools/bin/sqlcmd -S 127.0.0.1,1433 -U SA

# Localhost with explicit port
/opt/mssql-tools/bin/sqlcmd -S localhost,1433 -U SA

# Using machine name
/opt/mssql-tools/bin/sqlcmd -S $(hostname) -U SA
```

## Security Considerations

1. **Password Strength**: Use strong passwords that meet SQL Server complexity requirements:
   - At least 8 characters
   - Contains uppercase and lowercase letters
   - Contains numbers
   - Contains special characters

2. **Password Storage**: Avoid putting passwords in command history:
   ```bash
   # Use password prompt instead of -P parameter
   /opt/mssql-tools/bin/sqlcmd -S 127.0.0.1 -U SA
   ```

3. **Network Security**: SQL Server is configured to listen on all interfaces. In production:
   - Configure firewall rules
   - Use SSL/TLS encryption
   - Consider network-level security

## Environment Details

- **OS**: Ubuntu 24.04 (Noble) in WSL
- **SQL Server Version**: Microsoft SQL Server 2022 (RTM-CU21)
- **Edition**: Developer Edition
- **Architecture**: x86_64
- **Authentication Mode**: Mixed Mode (Windows + SQL Server Authentication)

## Prevention for Future Installations

To avoid this issue in future SQL Server installations:

1. **Always run `mssql-conf setup`** immediately after installing SQL Server
2. **Set strong SA password** during initial configuration
3. **Test connection** immediately after setup
4. **Document the SA password** securely
5. **Use IP address (`127.0.0.1`)** for initial connection tests

## Related Documentation

- [SQL Server WSL Startup Issue Solution](SQL-Server-WSL-Startup-Issue-Solution.md)
- [Microsoft SQL Server on Linux Documentation](https://docs.microsoft.com/en-us/sql/linux)
- [sqlcmd Utility Documentation](https://docs.microsoft.com/en-us/sql/tools/sqlcmd-utility)

---

**Resolution Date**: October 5, 2025  
**Tested On**: Ubuntu 24.04 (Noble) in WSL  
**Status**: ✅ Resolved - SQL Server connection working successfully

## Quick Reference Commands

```bash
# Check service status
sudo systemctl status mssql-server --no-pager

# Connect to SQL Server
/opt/mssql-tools/bin/sqlcmd -S 127.0.0.1 -U SA

# Execute quick query
/opt/mssql-tools/bin/sqlcmd -S 127.0.0.1 -U SA -P 'YourStrong!Passw0rd' -Q "SELECT @@VERSION"

# Check SQL Server configuration
sudo cat /var/opt/mssql/mssql.conf

# View error logs
sudo tail -20 /var/opt/mssql/log/errorlog
```