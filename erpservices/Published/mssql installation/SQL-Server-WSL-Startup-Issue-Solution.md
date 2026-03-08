# SQL Server WSL Startup Issue Solution

## Problem Description

When attempting to start SQL Server 2022 on Ubuntu 24.04 (Noble) in WSL, the service failed with the following error:

```bash
× mssql-server.service - Microsoft SQL Server Database Engine
     Loaded: loaded (/usr/lib/systemd/system/mssql-server.service; enabled; preset: enabled)
     Active: failed (Result: exit-code) since Sun 2025-10-05 22:48:27 +06; 54s ago
   Duration: 5ms
       Docs: https://docs.microsoft.com/en-us/sql/linux
   Main PID: 485415 (code=exited, status=127)
```

**Key indicators:**
- Exit code: 127 ("Command not found")
- Service fails to start repeatedly
- No detailed error logs available

## Root Cause Analysis

The issue was caused by **missing LDAP library dependencies** that SQL Server 2022 requires but are not available in Ubuntu 24.04 by default.

### Diagnostic Steps Performed

1. **Checked SQL Server logs**: `/var/opt/mssql/log/errorlog` was missing (service never started)
2. **Verified SQL Server configuration**: Configuration was properly set up
3. **Checked binary dependencies**: Used `ldd` command to identify missing libraries

```bash
ldd /opt/mssql/bin/sqlservr
```

**Result:** Found missing dependencies:
- `liblber-2.4.so.2 => not found`
- `libldap_r-2.4.so.2 => not found`

4. **System requirements check**: Memory and other requirements were satisfied

## The Solution

The solution involved installing the required LDAP libraries that SQL Server 2022 was built against, which are not available in Ubuntu 24.04's default repositories.

### Step-by-Step Fix

#### 1. Download Required LDAP Library
```bash
curl -O http://archive.ubuntu.com/ubuntu/pool/main/o/openldap/libldap-2.4-2_2.4.49+dfsg-2ubuntu1.10_amd64.deb
```

#### 2. Install Missing Dependencies
First, install the Heimdal dependencies required by the LDAP library:
```bash
sudo apt --fix-broken install
```

This will automatically install:
- `libasn1-8t64-heimdal`
- `libgssapi3t64-heimdal`
- `libhcrypto5t64-heimdal`
- `libheimbase1t64-heimdal`
- `libheimntlm0t64-heimdal`
- `libhx509-5t64-heimdal`
- `libkrb5-26t64-heimdal`
- `libroken19t64-heimdal`
- `libwind0t64-heimdal`

#### 3. Install the LDAP Library
```bash
sudo dpkg -i libldap-2.4-2_2.4.49+dfsg-2ubuntu1.10_amd64.deb
```

#### 4. Verify Dependencies are Resolved
```bash
ldd /opt/mssql/bin/sqlservr | grep "not found"
```
*Should return no results if successful*

#### 5. Start SQL Server
```bash
sudo systemctl start mssql-server
```

#### 6. Verify SQL Server Status
```bash
sudo systemctl status mssql-server --no-pager
```

## Verification of Success

After applying the fix, SQL Server should show:

```bash
● mssql-server.service - Microsoft SQL Server Database Engine
     Loaded: loaded (/usr/lib/systemd/system/mssql-server.service; enabled; preset: enabled)
     Active: active (running) since Sun 2025-10-05 22:59:22 +06; 14s ago
       Docs: https://docs.microsoft.com/en-us/sql/linux
   Main PID: 491247 (sqlservr)
      Tasks: 1
     Memory: 937.3M
        CPU: 1.482s
     CGroup: /system.slice/mssql-server.service
             └─491247 /opt/mssql/bin/sqlservr
```

## Why This Issue Occurs

This issue specifically affects Ubuntu 24.04 (Noble) because:

1. **Library Version Mismatch**: SQL Server 2022 was compiled against OpenLDAP 2.4.x libraries
2. **Ubuntu 24.04 Changes**: The newer Ubuntu version uses OpenLDAP 2.6.x by default
3. **Missing Compatibility**: Ubuntu 24.04 doesn't include the older 2.4.x LDAP libraries in its repositories
4. **WSL Environment**: The issue is more common in WSL environments where minimal packages are installed

## Alternative Solutions (Not Recommended)

### Symbolic Link Approach (Unsuccessful)
We initially tried creating symbolic links to newer libraries:
```bash
sudo ln -sf /usr/lib/x86_64-linux-gnu/libldap.so.2 /usr/lib/x86_64-linux-gnu/libldap_r-2.4.so.2
sudo ln -sf /usr/lib/x86_64-linux-gnu/liblber.so.2 /usr/lib/x86_64-linux-gnu/liblber-2.4.so.2
```

**Why it failed**: Version symbol mismatch (`OPENLDAP_2.4_2` not found in newer libraries)

## Prevention for Future Installations

To avoid this issue on fresh Ubuntu 24.04 installations:

1. Install the LDAP compatibility package immediately after installing SQL Server
2. Consider using Ubuntu 22.04 LTS for SQL Server installations if possible
3. Always check library dependencies after SQL Server installation but before first startup

## Testing the Connection

After SQL Server is running, test the connection:

```bash
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA
```

## Environment Details

- **OS**: Ubuntu 24.04 (Noble) in WSL
- **SQL Server Version**: Microsoft SQL Server 2022
- **Architecture**: x86_64
- **Memory**: 3.8GB available (meets minimum 2GB requirement)

## Related Issues

This solution addresses the common "exit code 127" error when starting SQL Server on Ubuntu 24.04. Similar issues may occur with:
- Other applications compiled against older OpenLDAP versions
- Different SQL Server versions on newer Ubuntu releases
- Container deployments without proper base image library versions

## References

- [Microsoft SQL Server on Linux Documentation](https://docs.microsoft.com/en-us/sql/linux)
- [OpenLDAP Library Documentation](https://www.openldap.org/)
- [Ubuntu Package Repository](http://archive.ubuntu.com/ubuntu/pool/main/o/openldap/)

---

**Resolution Date**: October 5, 2025  
**Tested On**: Ubuntu 24.04 (Noble) in WSL  
**Status**: ✅ Resolved - SQL Server successfully running