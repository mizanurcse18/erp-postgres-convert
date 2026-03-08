#!/bin/bash

# ERP Services Startup Script for WSL
# This script starts the core ERP services with proper environment setup

echo "=========================================="
echo "Starting ERP Services"  
echo "=========================================="

# Set proper environment paths and variables
export PATH=/usr/bin:/bin:/usr/sbin:/sbin:/home/munnacse18/.dotnet:$PATH
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_HTTPS_PORT=''

# Get current directory
SCRIPT_DIR="$(pwd)"
echo "Working from: $SCRIPT_DIR"

# Kill any existing processes
echo "Stopping existing services..."
/usr/bin/pkill -f '\.dll' 2>/dev/null || true
sleep 3

echo "Starting services..."

# Start APIGateway
cd "$SCRIPT_DIR/APIGateway"
export ASPNETCORE_URLS='http://0.0.0.0:5000'
/usr/bin/nohup /home/munnacse18/.dotnet/dotnet APIGateway.dll > apigateway.log 2>&1 &
API_PID=$!
echo "✓ Started APIGateway on port 5000 (PID: $API_PID)"

# Start Security.API
cd "$SCRIPT_DIR/Security.API"  
export ASPNETCORE_URLS='http://0.0.0.0:5001'
/usr/bin/nohup /home/munnacse18/.dotnet/dotnet Security.API.dll > security.api.log 2>&1 &
SECURITY_PID=$!
echo "✓ Started Security.API on port 5001 (PID: $SECURITY_PID)"

# Start Approval.API
cd "$SCRIPT_DIR/Approval.API"
export ASPNETCORE_URLS='http://0.0.0.0:5002'
/usr/bin/nohup /home/munnacse18/.dotnet/dotnet Approval.API.dll > approval.api.log 2>&1 &
APPROVAL_PID=$!
echo "✓ Started Approval.API on port 5002 (PID: $APPROVAL_PID)"

cd "$SCRIPT_DIR"

# Wait for services to initialize
echo ""
echo "Waiting for services to initialize..."
sleep 10

echo ""
echo "===========================================" 
echo "Service Status Check"
echo "==========================================="

# Check which processes are running
/usr/bin/pgrep -f 'APIGateway.dll' > /dev/null && echo "✅ APIGateway is running" || echo "❌ APIGateway failed to start"
/usr/bin/pgrep -f 'Security.API.dll' > /dev/null && echo "✅ Security.API is running" || echo "❌ Security.API failed to start"
/usr/bin/pgrep -f 'Approval.API.dll' > /dev/null && echo "✅ Approval.API is running" || echo "❌ Approval.API failed to start"

echo ""
echo "Port Status:"
if ss -tln 2>/dev/null | grep -E ':(5000|5001|5002)' > /dev/null; then
    echo "✅ Services are listening on expected ports:"
    ss -tln | grep -E ':(5000|5001|5002)'
else
    echo "⚠️  No services listening on expected ports 5000-5002"
fi

echo ""
echo "==========================================="
echo "Service URLs"
echo "==========================================="
echo "APIGateway:   http://localhost:5000"
echo "Security.API: http://localhost:5001"  
echo "Approval.API: http://localhost:5002"

echo ""
echo "Log Files:"
echo "APIGateway:   $SCRIPT_DIR/APIGateway/apigateway.log"
echo "Security.API: $SCRIPT_DIR/Security.API/security.api.log"
echo "Approval.API: $SCRIPT_DIR/Approval.API/approval.api.log"

echo ""
echo "To stop services: pkill -f '.dll'"
echo "To view logs: tail -f APIGateway/apigateway.log"
echo "==========================================="
