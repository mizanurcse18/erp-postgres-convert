#!/bin/bash
# Ubuntu Deployment Commands

# Install .NET runtimes
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-3.1 aspnetcore-runtime-5.0

# Create application directory
sudo mkdir -p /home/munnacse18/apps/back-end-services
sudo chown $USER:$USER /home/munnacse18/apps/back-end-services

echo "Deployment setup completed!"
