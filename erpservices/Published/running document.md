# ERP Services Quick Start Guide

## Overview

This guide covers how to run the ERP services using the updated `quick-start.sh` script. The script has been modified to run services directly from the current directory instead of copying them to `/opt/nagad-services`.

## Prerequisites

- Ubuntu/WSL environment
- .NET Core runtime (script will install if missing)
- Service directories (APIGateway, Security.API, Approval.API) in the current directory

## Script Features

The `quick-start.sh` script provides the following functionality:

### 1. Automatic Setup
- Installs .NET Core runtime if not present
- Verifies service directories exist
- Sets proper file permissions
- Tests services before starting

### 2. Service Management
- Start/stop services with a single command
- View service status and logs
- Kill processes cleanly when stopping

### 3. Interactive and CLI Modes
- Interactive menu when run without parameters
- Command-line options for automation

## Usage

### Copy Services to WSL (if needed)

If you need to copy the Published folder to WSL:

```bash
# From Windows PowerShell
cp -r "D:\SourceCode\Opseek\source\ERP\erpservices\Published" \\wsl$\Ubuntu\home\username\apps\back-end-services
```

### Running the Script

Make sure you're in the directory containing your service folders before running:

```bash
cd /home/username/apps/back-end-services  # or your service directory
```

#### Interactive Mode

```bash
./quick-start.sh
```

This will show a menu with options:
1. Full setup (install dependencies, verify services, start services)
2. Start services
3. Stop services  
4. Show service status
5. Test services only
6. Exit

#### Command Line Mode

```bash
# Full setup and start
./quick-start.sh setup

# Start services only
./quick-start.sh start

# Stop all services
./quick-start.sh stop

# Check service status
./quick-start.sh status

# Test services without starting
./quick-start.sh test
```

## Service Configuration

The script manages these services by default:

| Service | Port | Log File |
|---------|------|----------|
| APIGateway | 5000 | apigateway.log |
| Security.API | 5001 | security.api.log |
| Approval.API | 5002 | approval.api.log |

Optional services (HRMS.API, Mail.API) are detected but not required.

## Directory Structure

Your working directory should contain:

```
/current/directory/
├── quick-start.sh
├── APIGateway/
│   ├── APIGateway.dll
│   └── [other files]
├── Security.API/
│   ├── Security.API.dll
│   └── [other files]
├── Approval.API/
│   ├── Approval.API.dll
│   └── [other files]
├── HRMS.API/ (optional)
└── Mail.API/ (optional)
```

## Logs and Monitoring

### Log Files

Each service creates its own log file in its directory:
- `APIGateway/apigateway.log`
- `Security.API/security.api.log`
- `Approval.API/approval.api.log`

### Real-time Log Monitoring

```bash
# Watch all logs
tail -f APIGateway/apigateway.log Security.API/security.api.log Approval.API/approval.api.log

# Watch specific service
tail -f APIGateway/apigateway.log
```

### Service Status

Check if services are running:

```bash
./quick-start.sh status
```

Or manually:

```bash
# Check processes
ps aux | grep dotnet

# Check network ports
netstat -tlpn | grep -E ':(5000|5001|5002)'
```

## Troubleshooting

### Script Won't Start

1. **Check permissions:**
   ```bash
   chmod +x quick-start.sh
   ```

2. **Check line endings (if copied from Windows):**
   ```bash
   dos2unix quick-start.sh
   ```

3. **Check you're in the right directory:**
   ```bash
   ls -la
   # Should see APIGateway, Security.API, Approval.API directories
   ```

### Services Won't Start

1. **Check .NET installation:**
   ```bash
   dotnet --version
   ```

2. **Test individual service:**
   ```bash
   cd APIGateway
   dotnet APIGateway.dll
   ```

3. **Check for port conflicts:**
   ```bash
   netstat -tlpn | grep -E ':(5000|5001|5002)'
   ```

### Performance Issues

1. **Check system resources:**
   ```bash
   top
   free -h
   df -h
   ```

2. **Monitor logs for errors:**
   ```bash
   tail -f APIGateway/apigateway.log | grep -i error
   ```

## Service URLs

Once running, services are available at:

- **API Gateway:** http://localhost:5000
- **Security API:** http://localhost:5001  
- **Approval API:** http://localhost:5002

### Health Checks

Test if services are responding:

```bash
curl http://localhost:5000/health  # API Gateway
curl http://localhost:5001/health  # Security API
curl http://localhost:5002/health  # Approval API
```

## Stopping Services

### Using Script

```bash
./quick-start.sh stop
```

### Manual Stop

```bash
# Kill all .NET processes
pkill -f "\.dll"

# Or kill specific services
pkill -f "APIGateway.dll"
pkill -f "Security.API.dll"
pkill -f "Approval.API.dll"
```

## Production Considerations

For production deployment, consider:

1. **Use a process manager** (systemd, PM2)
2. **Configure proper logging** (structured logs, log rotation)
3. **Set environment variables** for configuration
4. **Use a reverse proxy** (nginx, Apache)
5. **Implement health monitoring**
6. **Configure SSL/TLS**

## Script Updates

The script has been updated from the original version to:

- ✅ Run services from current directory (no copying to `/opt/nagad-services`)
- ✅ Better error handling and user feedback
- ✅ Cleaner log management
- ✅ More robust service detection
- ✅ Command-line interface support
- ✅ Working directory awareness

## Support

For issues with:
- **Script functionality:** Check this documentation and troubleshooting section
- **Service configuration:** Check individual service logs and .NET requirements  
- **Network connectivity:** Verify ports and firewall settings
- **Performance:** Monitor system resources and optimize accordingly
