#!/bin/bash
# Quick start script for ERP Services on Ubuntu - Updated to run from current directory

echo "==================================="
echo "ERP Services Quick Start"
echo "==================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Setup .NET environment
export PATH="/home/munnacse18/.dotnet:$PATH"
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Get current directory
CURRENT_DIR=$(pwd)
echo -e "${YELLOW}Working from: ${CURRENT_DIR}${NC}"
echo -e "${YELLOW}Using .NET from: /home/munnacse18/.dotnet${NC}"

# Check if running as root
if [ "$EUID" -eq 0 ]; then 
  echo -e "${RED}Please don't run this script as root${NC}"
  exit 1
fi

# Function to check if .NET is installed
check_dotnet() {
    echo -e "${YELLOW}Checking .NET runtime...${NC}"
    if command -v dotnet &> /dev/null; then
        echo -e "${GREEN}✅ .NET is available${NC}"
        dotnet --list-runtimes | grep -E "(3\.1|Microsoft\.AspNetCore\.App)"
    else
        echo -e "${RED}❌ .NET runtime not found in PATH${NC}"
        echo -e "${YELLOW}Please ensure .NET 3.1 runtime is installed${NC}"
        return 1
    fi
}

# Function to verify services in current directory
verify_services() {
    echo -e "${YELLOW}Verifying services in current directory...${NC}"
    
    if [ -d "APIGateway" ] && [ -d "Security.API" ] && [ -d "Approval.API" ] && [ -d "HRMS.API" ]; then
        echo -e "${GREEN}✅ Core services found in current directory${NC}"
        
        # Optional services
        [ -d "Mail.API" ] && echo -e "${GREEN}✅ Mail.API found${NC}" || echo -e "${YELLOW}⚠️ Mail.API not found (optional)${NC}"
        [ -d "SCM.API" ] && echo -e "${GREEN}✅ SCM.API found${NC}" || echo -e "${YELLOW}⚠️ SCM.API not found (optional)${NC}"
        
        return 0
    else
        echo -e "${RED}❌ Required service folders not found in current directory${NC}"
        echo "Expected folders: APIGateway, Security.API, Approval.API, HRMS.API"
        echo "Please run this script from the directory containing your service folders."
        return 1
    fi
}

# Function to set permissions
set_permissions() {
    echo -e "${YELLOW}Setting file permissions...${NC}"
    find . -name "*.dll" -exec chmod +x {} \; 2>/dev/null || true
    echo -e "${GREEN}✅ Permissions set${NC}"
}

# Function to test services
test_services() {
    echo -e "${YELLOW}Testing services in current directory...${NC}"
    
    services=("APIGateway" "Security.API" "Approval.API" "HRMS.API")
    ports=(5000 5001 5002 5003)
    original_dir="$(pwd)"
    
    for i in "${!services[@]}"; do
        service_name="${services[$i]}"
        port="${ports[$i]}"
        
        if [ -d "$service_name" ]; then
            echo -e "Testing ${service_name}..."
            cd "$service_name"
            
            if [ -f "${service_name}.dll" ]; then
                echo -e "${GREEN}✅ ${service_name}.dll found${NC}"
                # Quick test to see if it can start (will exit immediately)
                timeout 3s dotnet "${service_name}.dll" --urls "http://localhost:${port}" &>/dev/null
                if [ $? -eq 124 ]; then  # timeout exit code means it started successfully
                    echo -e "${GREEN}✅ ${service_name} can start successfully${NC}"
                else
                    echo -e "${YELLOW}⚠️ ${service_name} may have configuration issues${NC}"
                fi
            else
                echo -e "${RED}❌ ${service_name}.dll not found${NC}"
            fi
            cd "$original_dir"
        else
            echo -e "${RED}❌ ${service_name} directory not found in current directory${NC}"
        fi
    done
}

# Function to start services
start_services() {
    echo -e "${YELLOW}Starting services in background...${NC}"
    
    services=("APIGateway" "Security.API" "Approval.API" "HRMS.API")
    ports=(5000 5001 5002 5003)
    original_dir="$(pwd)"
    
    for i in "${!services[@]}"; do
        service_name="${services[$i]}"
        port="${ports[$i]}"
        
        if [ -d "$service_name" ]; then
            cd "$service_name"
            
            # Kill any existing process on this port
            pkill -f "${service_name}.dll" 2>/dev/null || true
            sleep 2
            
            # Start the service with proper environment
            export ASPNETCORE_URLS="http://0.0.0.0:${port}"
            export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
            nohup dotnet "${service_name}.dll" > "${service_name,,}.log" 2>&1 &
            
            echo -e "${GREEN}✅ Started ${service_name} on port ${port}${NC}"
            cd "$original_dir"
        else
            echo -e "${RED}❌ ${service_name} directory not found in current directory${NC}"
        fi
    done
    
    echo -e "\n${GREEN}All services started!${NC}"
    echo -e "Logs are available in each service directory:"
    echo -e "  - API Gateway: ${original_dir}/APIGateway/apigateway.log"
    echo -e "  - Security API: ${original_dir}/Security.API/security.api.log"
    echo -e "  - Approval API: ${original_dir}/Approval.API/approval.api.log"
    echo -e "  - HRMS API: ${original_dir}/HRMS.API/hrms.api.log"
}

# Function to show status
show_status() {
    echo -e "\n${YELLOW}Service Status:${NC}"
    echo "===================="
    
    services=("APIGateway" "Security.API" "Approval.API" "HRMS.API")
    ports=(5000 5001 5002 5003)
    
    for i in "${!services[@]}"; do
        service_name="${services[$i]}"
        port="${ports[$i]}"
        
        if pgrep -f "${service_name}.dll" > /dev/null; then
            echo -e "${GREEN}✅ ${service_name} is running on port ${port}${NC}"
        else
            echo -e "${RED}❌ ${service_name} is not running${NC}"
        fi
    done
    
    echo -e "\n${YELLOW}Network Status:${NC}"
    netstat -tlpn 2>/dev/null | grep -E ':(5000|5001|5002|5003)' || echo "No services listening on expected ports"
}

# Function to stop services
stop_services() {
    echo -e "${YELLOW}Stopping all services...${NC}"
    
    services=("APIGateway" "Security.API" "Approval.API" "HRMS.API")
    
    for service_name in "${services[@]}"; do
        pkill -f "${service_name}.dll" 2>/dev/null && echo -e "${GREEN}✅ Stopped ${service_name}${NC}" || echo -e "${YELLOW}⚠️ ${service_name} was not running${NC}"
    done
}

# Main menu
show_menu() {
    echo -e "\n${YELLOW}Choose an option:${NC}"
    echo "1) Full setup (install dependencies, verify services, start services)"
    echo "2) Start services"
    echo "3) Stop services"
    echo "4) Show service status"
    echo "5) Test services only"
    echo "6) Exit"
    echo -n "Enter your choice [1-6]: "
}

# Main execution
main() {
    case $1 in
        "setup" | "1")
            check_dotnet
            verify_services || exit 1
            set_permissions
            test_services
            start_services
            show_status
            ;;
        "start" | "2")
            verify_services || exit 1
            start_services
            show_status
            ;;
        "stop" | "3")
            stop_services
            ;;
        "status" | "4")
            show_status
            ;;
        "test" | "5")
            verify_services || exit 1
            test_services
            ;;
        *)
            if [ -z "$1" ]; then
                show_menu
                read -r choice
                main "$choice"
            else
                echo "Usage: $0 [setup|start|stop|status|test]"
                exit 1
            fi
            ;;
    esac
}

# Run main function with all arguments
main "$@"
