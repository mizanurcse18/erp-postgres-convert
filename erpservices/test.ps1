Write-Host "Test script starting..." -ForegroundColor Green

$OutputDir = "D:\SourceCode\nagad\backend-report\nagaderpservices\Published"

$UbuntuCommands = @'
#!/bin/bash
# Test commands
sudo chown $USER:$USER /home/test
'@

Write-Host "Test completed successfully!" -ForegroundColor Green