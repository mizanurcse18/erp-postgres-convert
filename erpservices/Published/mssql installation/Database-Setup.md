# Database Setup Guide - ERP System

## Overview
This guide covers database creation, import, and configuration procedures for the ERP system running on SQL Server in WSL.

## Prerequisites
- ✅ SQL Server installed and running in WSL
- ✅ SQL Server tools (sqlcmd) installed
- ✅ Network connectivity between Windows and WSL established
- ✅ SA or appropriate database user credentials

## Database Setup Options

### Option 1: Create New Database from Scratch

#### Step 1: Connect to SQL Server
```bash
# From WSL Ubuntu
sqlcmd -S localhost -U sa
```

#### Step 2: Create ERP Database
```sql
-- Create the main ERP database
CREATE DATABASE ERPDatabase
ON (
    NAME = 'ERPDatabase_Data',
    FILENAME = '/var/opt/mssql/data/ERPDatabase.mdf',
    SIZE = 100MB,
    MAXSIZE = 10GB,
    FILEGROWTH = 10MB
)
LOG ON (
    NAME = 'ERPDatabase_Log',
    FILENAME = '/var/opt/mssql/data/ERPDatabase.ldf',
    SIZE = 10MB,
    MAXSIZE = 1GB,
    FILEGROWTH = 10%
);
GO

-- Verify database creation
SELECT name FROM sys.databases WHERE name = 'ERPDatabase';
GO

USE ERPDatabase;
GO

-- Check database status
SELECT 
    name,
    database_id,
    create_date,
    state_desc
FROM sys.databases 
WHERE name = 'ERPDatabase';
GO

quit
```

### Option 2: Restore Database from Backup

#### Step 1: Copy Backup File to WSL

If you have a `.bak` file on Windows:

```bash
# From WSL, access Windows files
# Windows D: drive is available at /mnt/d/ in WSL
ls /mnt/d/SourceCode/Opseek/source/ERP/erpservices/Published/

# Copy backup file to SQL Server data directory
sudo cp /mnt/d/path/to/your/database.bak /var/opt/mssql/data/

# Set proper permissions
sudo chown mssql:mssql /var/opt/mssql/data/database.bak
```

#### Step 2: Restore Database
```sql
-- Connect to SQL Server
sqlcmd -S localhost -U sa

-- Get backup information
RESTORE HEADERONLY FROM DISK = '/var/opt/mssql/data/database.bak';
GO

-- Get file list from backup
RESTORE FILELISTONLY FROM DISK = '/var/opt/mssql/data/database.bak';
GO

-- Restore database
RESTORE DATABASE ERPDatabase
FROM DISK = '/var/opt/mssql/data/database.bak'
WITH 
    MOVE 'Original_Data_Name' TO '/var/opt/mssql/data/ERPDatabase.mdf',
    MOVE 'Original_Log_Name' TO '/var/opt/mssql/data/ERPDatabase.ldf',
    REPLACE;
GO

quit
```

### Option 3: Execute SQL Scripts

If you have SQL scripts (`.sql` files):

#### Step 1: Copy SQL Scripts
```bash
# Copy SQL scripts from Windows to WSL
sudo cp /mnt/d/SourceCode/Opseek/source/ERP/erpservices/Published/*.sql /tmp/

# Set permissions
sudo chmod 644 /tmp/*.sql
```

#### Step 2: Execute Scripts
```bash
# Execute SQL scripts in order
sqlcmd -S localhost -U sa -d ERPDatabase -i /tmp/create_tables.sql
sqlcmd -S localhost -U sa -d ERPDatabase -i /tmp/insert_data.sql
sqlcmd -S localhost -U sa -d ERPDatabase -i /tmp/create_procedures.sql
```

## ERP System Database Configuration

### Create Application-Specific Users

```sql
-- Connect as SA
sqlcmd -S localhost -U sa

-- Create dedicated users for each API service
CREATE LOGIN AccountsAPIUser WITH PASSWORD = 'Acc0unts_P@ss123';
CREATE LOGIN SecurityAPIUser WITH PASSWORD = 'S3cur1ty_P@ss123';
CREATE LOGIN HRMSAPIUser WITH PASSWORD = 'HRMS_P@ss123';
CREATE LOGIN SCMAPIUser WITH PASSWORD = 'SCM_P@ss123';
CREATE LOGIN ApprovalAPIUser WITH PASSWORD = 'Appr0val_P@ss123';
CREATE LOGIN MailAPIUser WITH PASSWORD = 'M@il_P@ss123';
GO

-- Use the ERP database
USE ERPDatabase;
GO

-- Create users for each login
CREATE USER AccountsAPIUser FOR LOGIN AccountsAPIUser;
CREATE USER SecurityAPIUser FOR LOGIN SecurityAPIUser;
CREATE USER HRMSAPIUser FOR LOGIN HRMSAPIUser;
CREATE USER SCMAPIUser FOR LOGIN SCMAPIUser;
CREATE USER ApprovalAPIUser FOR LOGIN ApprovalAPIUser;
CREATE USER MailAPIUser FOR LOGIN MailAPIUser;
GO

-- Assign appropriate roles
ALTER ROLE db_datareader ADD MEMBER AccountsAPIUser;
ALTER ROLE db_datawriter ADD MEMBER AccountsAPIUser;
ALTER ROLE db_datareader ADD MEMBER SecurityAPIUser;
ALTER ROLE db_datawriter ADD MEMBER SecurityAPIUser;
ALTER ROLE db_datareader ADD MEMBER HRMSAPIUser;
ALTER ROLE db_datawriter ADD MEMBER HRMSAPIUser;
ALTER ROLE db_datareader ADD MEMBER SCMAPIUser;
ALTER ROLE db_datawriter ADD MEMBER SCMAPIUser;
ALTER ROLE db_datareader ADD MEMBER ApprovalAPIUser;
ALTER ROLE db_datawriter ADD MEMBER ApprovalAPIUser;
ALTER ROLE db_datareader ADD MEMBER MailAPIUser;
ALTER ROLE db_datawriter ADD MEMBER MailAPIUser;
GO

quit
```

### Create Module-Specific Schemas (Optional)

```sql
-- Connect to database
sqlcmd -S localhost -U sa -d ERPDatabase

-- Create schemas for different modules
CREATE SCHEMA Accounts;
CREATE SCHEMA Security;
CREATE SCHEMA HRMS;
CREATE SCHEMA SCM;
CREATE SCHEMA Approval;
CREATE SCHEMA Mail;
GO

-- Grant schema permissions
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Accounts TO AccountsAPIUser;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Security TO SecurityAPIUser;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::HRMS TO HRMSAPIUser;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::SCM TO SCMAPIUser;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Approval TO ApprovalAPIUser;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Mail TO MailAPIUser;
GO

quit
```

## Connection Strings for ERP Services

### Get WSL IP Address
```bash
# From WSL
WSL_IP=$(hostname -I | awk '{print $1}')
echo "WSL IP: $WSL_IP"
```

### Connection String Templates

Update your `appsettings.json` files for each API service:

#### Accounts.API
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=AccountsAPIUser;Password=Acc0unts_P@ss123;TrustServerCertificate=true;"
  }
}
```

#### Security.API
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=SecurityAPIUser;Password=S3cur1ty_P@ss123;TrustServerCertificate=true;"
  }
}
```

#### HRMS.API
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=HRMSAPIUser;Password=HRMS_P@ss123;TrustServerCertificate=true;"
  }
}
```

#### SCM.API
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=SCMAPIUser;Password=SCM_P@ss123;TrustServerCertificate=true;"
  }
}
```

#### Approval.API
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=ApprovalAPIUser;Password=Appr0val_P@ss123;TrustServerCertificate=true;"
  }
}
```

#### Mail.API
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.68.113;Database=ERPDatabase;User Id=MailAPIUser;Password=M@il_P@ss123;TrustServerCertificate=true;"
  }
}
```

## Database Initialization Scripts

### Create Common Tables Script
Save as `create_common_tables.sql`:

```sql
USE ERPDatabase;
GO

-- System configuration table
CREATE TABLE SystemConfig (
    ConfigId INT IDENTITY(1,1) PRIMARY KEY,
    ConfigKey NVARCHAR(100) NOT NULL UNIQUE,
    ConfigValue NVARCHAR(500),
    Description NVARCHAR(255),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE()
);
GO

-- User management table
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    LastLoginDate DATETIME2 NULL
);
GO

-- Role management
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    IsActive BIT DEFAULT 1
);
GO

-- User roles mapping
CREATE TABLE UserRoles (
    UserRoleId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    RoleId INT FOREIGN KEY REFERENCES Roles(RoleId),
    AssignedDate DATETIME2 DEFAULT GETDATE()
);
GO

-- Insert default admin user
INSERT INTO Users (Username, Email, PasswordHash) 
VALUES ('admin', 'admin@erp.com', 'hashed_password_here');

INSERT INTO Roles (RoleName, Description) 
VALUES 
    ('Administrator', 'System Administrator'),
    ('Manager', 'Department Manager'),
    ('User', 'Regular User');
GO

PRINT 'Common tables created successfully';
```

### Execute Initialization
```bash
# Run the script
sqlcmd -S localhost -U sa -i create_common_tables.sql
```

## Testing Database Connectivity

### Test from WSL
```bash
# Test basic connection
sqlcmd -S localhost -U sa -Q "SELECT @@VERSION"

# Test specific user connection
sqlcmd -S localhost -U AccountsAPIUser -P "Acc0unts_P@ss123" -Q "SELECT DB_NAME()"

# Test table access
sqlcmd -S localhost -U AccountsAPIUser -P "Acc0unts_P@ss123" -Q "SELECT COUNT(*) FROM Users"
```

### Test from Windows
```cmd
REM Test from Windows command prompt
sqlcmd -S 192.168.68.113 -U sa -Q "SELECT name FROM sys.databases"

REM Test application user
sqlcmd -S 192.168.68.113 -U AccountsAPIUser -P "Acc0unts_P@ss123" -Q "SELECT @@SERVERNAME"
```

### Test .NET Connection

Create a simple test console app:

```csharp
using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connectionString = "Server=192.168.68.113;Database=ERPDatabase;User Id=sa;Password=YourSAPassword;TrustServerCertificate=true;";
        
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connection successful!");
                
                SqlCommand command = new SqlCommand("SELECT @@VERSION", connection);
                string result = (string)command.ExecuteScalar();
                Console.WriteLine($"SQL Server Version: {result}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
        }
    }
}
```

## Performance Tuning

### Database Configuration
```sql
-- Connect as SA
sqlcmd -S localhost -U sa

-- Set database options for better performance
ALTER DATABASE ERPDatabase SET READ_COMMITTED_SNAPSHOT ON;
ALTER DATABASE ERPDatabase SET ALLOW_SNAPSHOT_ISOLATION ON;
ALTER DATABASE ERPDatabase SET AUTO_CREATE_STATISTICS ON;
ALTER DATABASE ERPDatabase SET AUTO_UPDATE_STATISTICS ON;
GO

-- Set recovery model (adjust based on your backup strategy)
ALTER DATABASE ERPDatabase SET RECOVERY SIMPLE;
GO

quit
```

### Index Optimization
```sql
-- Example: Create indexes for common queries
USE ERPDatabase;
GO

-- Index on Users table
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);
GO

-- Index on UserRoles
CREATE INDEX IX_UserRoles_UserId ON UserRoles(UserId);
CREATE INDEX IX_UserRoles_RoleId ON UserRoles(RoleId);
GO
```

## Backup Strategy

### Create Backup Directory
```bash
# Create backup directory
sudo mkdir -p /var/opt/mssql/backup
sudo chown mssql:mssql /var/opt/mssql/backup
```

### Manual Backup
```sql
-- Full backup
BACKUP DATABASE ERPDatabase 
TO DISK = '/var/opt/mssql/backup/ERPDatabase_Full.bak'
WITH FORMAT, INIT, COMPRESSION;
GO

-- Differential backup
BACKUP DATABASE ERPDatabase 
TO DISK = '/var/opt/mssql/backup/ERPDatabase_Diff.bak'
WITH DIFFERENTIAL, FORMAT, INIT, COMPRESSION;
GO
```

### Automated Backup Script
Save as `backup_erp.sh`:

```bash
#!/bin/bash
BACKUP_DIR="/var/opt/mssql/backup"
DATE=$(date +%Y%m%d_%H%M%S)

# Full backup
sqlcmd -S localhost -U sa -Q "BACKUP DATABASE ERPDatabase TO DISK = '$BACKUP_DIR/ERPDatabase_Full_$DATE.bak' WITH FORMAT, INIT, COMPRESSION;"

# Remove backups older than 7 days
find $BACKUP_DIR -name "ERPDatabase_*.bak" -mtime +7 -delete

echo "Backup completed: ERPDatabase_Full_$DATE.bak"
```

Make it executable:
```bash
chmod +x backup_erp.sh
```

## Troubleshooting

### Common Issues

1. **Connection Refused**
   ```bash
   # Check if SQL Server is running
   sudo systemctl status mssql-server --no-pager
   
   # Check if port 1433 is listening
   sudo netstat -tuln | grep 1433
   ```

2. **Permission Denied**
   ```sql
   -- Check user permissions
   SELECT 
       dp.class_desc,
       dp.permission_name,
       dp.state_desc,
       pr.name AS principal_name
   FROM sys.database_permissions dp
   JOIN sys.database_principals pr ON dp.grantee_principal_id = pr.principal_id
   WHERE pr.name = 'AccountsAPIUser';
   ```

3. **Database Not Found**
   ```sql
   -- List all databases
   SELECT name FROM sys.databases;
   ```

## Next Steps

1. ✅ Set up database structure
2. 🔧 Configure connection strings in all API services
3. 🚀 Test database connectivity from each service
4. 📊 Set up monitoring and logging
5. 🔄 Implement backup and recovery procedures

---

**Note**: Replace IP addresses, passwords, and database names with your actual values.

**Version**: 1.0  
**Target**: ERP System Database Setup