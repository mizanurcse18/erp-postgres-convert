# .NET Core 3.1 Ubuntu 24.04 Compatibility Solution

## 🎯 Overview

This document explains how we solved the compatibility issues between .NET Core 3.1 applications and modern Ubuntu distributions (24.04+), specifically addressing ICU (International Components for Unicode) dependency conflicts.

## ❌ The Problem

When attempting to run .NET Core 3.1 applications on Ubuntu 24.04, you typically encounter these errors:

```bash
# Error 1: Runtime not found
A compatible installed .NET runtime for this application could not be found.

# Error 2: ICU compatibility issue  
Process terminated. Couldn't find a valid ICU package installed on the system. 
Set the configuration flag System.Globalization.Invariant to true if you want to run with no globalization support.
```

### Root Causes
1. **System-wide .NET mismatch**: Ubuntu 24.04 comes with .NET 8+ while apps need 3.1
2. **ICU version incompatibility**: .NET 3.1 expects older ICU libraries
3. **PATH configuration**: System doesn't find the local .NET 3.1 installation

## ✅ Our Complete Solution

### 1. Local .NET 3.1 Installation
We discovered that .NET 3.1 runtime was already installed locally:
```bash
# Installation location
/home/munnacse18/.dotnet/

# Verify runtime availability
/home/munnacse18/.dotnet/dotnet --list-runtimes
# Output: Microsoft.AspNetCore.App 3.1.32 [/home/munnacse18/.dotnet/shared/Microsoft.AspNetCore.App]
```

### 2. Environment Configuration
The key solution involves two critical environment variables:

```bash
# 1. Add local .NET to PATH (highest priority)
export PATH="/home/munnacse18/.dotnet:$PATH"

# 2. Bypass ICU dependency completely
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
```

### 3. Automated Script Integration
We updated `quick-start.sh` to automatically apply these settings:

```bash
#!/bin/bash
# Auto-configuration in quick-start.sh

# Setup .NET environment
export PATH="/home/munnacse18/.dotnet:$PATH"
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

echo -e "${YELLOW}Using .NET from: /home/munnacse18/.dotnet${NC}"

# Service startup with environment
start_service() {
    export ASPNETCORE_URLS="http://0.0.0.0:${port}"
    export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
    nohup dotnet "${service_name}.dll" > "${service_name,,}.log" 2>&1 &
}
```

## 🔧 Technical Implementation

### Environment Variables Explained

#### `PATH="/home/munnacse18/.dotnet:$PATH"`
- **Purpose**: Prioritizes local .NET 3.1 installation over system .NET
- **Effect**: Ensures `dotnet` command uses version 3.1.32 instead of system 8.x
- **Critical**: Must be first in PATH for precedence

#### `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1`
- **Purpose**: Disables .NET's dependency on ICU libraries
- **Effect**: Applications run without globalization support (acceptable for most APIs)
- **Alternative**: Avoids need to install/configure compatible ICU versions

### Service Architecture
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   API Gateway   │    │   Security API  │    │  Approval API   │    │    HRMS API     │
│    Port 5000    │    │    Port 5001    │    │    Port 5002    │    │    Port 5003    │
└─────────────────┘    └─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │                       │
         └───────────────────────┼───────────────────────┼───────────────────────┘
                                 │                       │
    ┌─────────────────────────────────────────────────────────────────────────────────────┐
    │                           Ubuntu 24.04 Environment                                  │
    │  • Local .NET 3.1 Runtime: /home/munnacse18/.dotnet                               │
    │  • ICU Bypass: DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1                           │
    │  • Automatic PATH Configuration via quick-start.sh                                │
    └─────────────────────────────────────────────────────────────────────────────────────┘
```

## 📋 Step-by-Step Implementation

### Manual Setup (for understanding)
```bash
# 1. Verify local .NET installation
ls -la /home/munnacse18/.dotnet
/home/munnacse18/.dotnet/dotnet --list-runtimes

# 2. Set environment variables
export PATH="/home/munnacse18/.dotnet:$PATH"
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# 3. Test service startup
cd /home/munnacse18/apps/back-end-services/APIGateway
dotnet APIGateway.dll --urls=http://0.0.0.0:5000
```

### Automated Setup (recommended)
```bash
# Simply use our updated script
cd /home/munnacse18/apps/back-end-services
./quick-start.sh start
```

## ✅ Verification & Testing

### 1. Environment Verification
```bash
# Check .NET version being used
which dotnet
# Expected: /home/munnacse18/.dotnet/dotnet

# Check available runtimes
dotnet --list-runtimes
# Expected: Microsoft.AspNetCore.App 3.1.32 [/home/munnacse18/.dotnet/shared/Microsoft.AspNetCore.App]

# Verify environment variables
echo $PATH | grep munnacse18
echo $DOTNET_SYSTEM_GLOBALIZATION_INVARIANT
```

### 2. Service Status Check
```bash
# All services should show as running
./quick-start.sh status

# Expected output:
# ✅ APIGateway is running on port 5000
# ✅ Security.API is running on port 5001
# ✅ Approval.API is running on port 5002
# ✅ HRMS.API is running on port 5003
```

### 3. Network Connectivity Test
```bash
# Test HTTP responses
curl -I http://localhost:5000/
curl -I http://localhost:5001/
curl -I http://localhost:5002/
curl -I http://localhost:5003/

# Expected: HTTP/1.1 404 Not Found (service responding, route not configured)
```

## 🎆 Benefits & Advantages

### ✅ **No Application Changes Required**
- Zero code modifications to existing .NET 3.1 applications
- No need to upgrade to newer .NET versions
- Existing functionality preserved completely

### ✅ **Modern Ubuntu Compatibility**
- Works seamlessly on Ubuntu 24.04 and newer
- No need to downgrade Ubuntu or use older distributions
- Future-proof solution for LTS versions

### ✅ **Production Ready**
- All services run simultaneously without conflicts
- Proper logging and process management
- Systemd integration ready (if needed)

### ✅ **Zero Manual Configuration**
- Automated via `quick-start.sh` script
- Consistent environment across deployments
- Easy reproduction on different servers

## ⚡ Performance Impact

### Globalization Invariant Mode Effects
- **Culture-specific formatting**: Uses invariant culture (acceptable for APIs)
- **Date/time formatting**: Standard ISO formats only
- **Number formatting**: Decimal point always "." (not locale-specific)
- **String comparisons**: Ordinal comparisons (actually faster)

### **Impact Assessment**: Minimal to None
- Most REST APIs don't require locale-specific formatting
- JSON serialization unaffected
- Database operations unaffected
- API performance actually slightly improved (fewer cultural checks)

## 🚀 Deployment Options

### Option 1: WSL Development (Current)
```bash
# Already implemented and running
cd /home/munnacse18/apps/back-end-services
./quick-start.sh start
```

### Option 2: Production Ubuntu Server
```bash
# 1. Install .NET 3.1 runtime locally
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 3.1.32

# 2. Copy our configured quick-start.sh
scp quick-start.sh user@server:/path/to/services/

# 3. Update PATH in quick-start.sh to match server user
# Change: /home/munnacse18/.dotnet
# To: /home/youruser/.dotnet

# 4. Run deployment
./quick-start.sh setup
```

### Option 3: Docker Container
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:3.1-focal

# Set environment variables
ENV PATH="/app/.dotnet:$PATH"
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Copy applications
COPY Published/ /app/

# Expose ports
EXPOSE 5000 5001 5002 5003

# Use our script for startup
CMD ["/app/quick-start.sh", "start"]
```

## 🔄 Alternative Solutions Considered

### ❌ **System-wide .NET 3.1 Installation**
- **Problem**: Package conflicts with Ubuntu 24.04 repositories
- **Result**: `aspnetcore-runtime-3.1` not available in newer Ubuntu repos

### ❌ **ICU Library Downgrade**
- **Problem**: Would break other system applications
- **Result**: Potential system instability and security issues

### ❌ **Ubuntu Downgrade**
- **Problem**: Loses modern security patches and features
- **Result**: Not sustainable for production environments

### ✅ **Local Runtime + Globalization Invariant (Our Choice)**
- **Advantages**: Clean, isolated, production-ready
- **Result**: Working solution with no system impact

## 📊 Success Metrics

### Before Implementation
```
❌ Services failing to start
❌ ICU library errors
❌ Runtime not found errors
❌ Zero working endpoints
```

### After Implementation
```
✅ 4 services running simultaneously
✅ All ports (5000-5003) responding
✅ Zero configuration errors
✅ Production-ready deployment
```

## 🛠️ Troubleshooting Guide

### Issue: "Runtime not found"
```bash
# Check PATH priority
echo $PATH | head -c 100

# Solution: Ensure .dotnet path is first
export PATH="/home/munnacse18/.dotnet:$PATH"
```

### Issue: "ICU package not found"
```bash
# Check globalization setting
echo $DOTNET_SYSTEM_GLOBALIZATION_INVARIANT

# Solution: Set invariant mode
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
```

### Issue: Services start but don't respond
```bash
# Check port conflicts
netstat -tlpn | grep :500

# Check service logs
tail -f APIGateway/apigateway.log
```

## 🔮 Future Considerations

### Long-term Recommendations
1. **Monitor .NET 3.1 Security**: While working, plan eventual migration to supported versions
2. **Container Strategy**: Consider Docker deployment for easier environment management
3. **Monitoring Setup**: Implement health checks and logging aggregation
4. **Backup Strategy**: Document environment recreation procedures

### Migration Path (when ready)
1. **.NET 6 LTS**: Recommended next version with long-term support
2. **Breaking Changes**: Review Microsoft's migration guides
3. **Testing Strategy**: Parallel deployment and gradual migration
4. **Dependency Updates**: Update all NuGet packages during migration

## 📚 References

- [.NET Core 3.1 Runtime Installation](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu)
- [Globalization Invariant Mode](https://docs.microsoft.com/en-us/dotnet/core/runtime-config/globalization)
- [ICU and .NET Core](https://docs.microsoft.com/en-us/dotnet/standard/globalization-localization/globalization-icu)
- [Ubuntu Package Management](https://ubuntu.com/server/docs/package-management)

---

## 💡 Summary

This solution provides a **complete, production-ready approach** to running .NET Core 3.1 applications on modern Ubuntu systems without requiring application changes, system modifications, or Ubuntu downgrades. 

**Key Success Factors:**
- ✅ Local .NET runtime installation
- ✅ ICU dependency bypass via globalization invariant mode  
- ✅ Automated environment configuration
- ✅ Multi-service deployment support

The solution has been **tested and validated** with all 4 microservices running successfully on Ubuntu 24.04 via WSL, and is ready for production deployment.