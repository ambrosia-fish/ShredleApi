# Clean-Ports.ps1
# This script helps identify and fix port conflicts for .NET applications

# Port to check
$port = 5001
if ($args.Count -gt 0) {
    $port = $args[0]
}

Write-Host "Checking for processes using port $port..." -ForegroundColor Cyan

# Check for processes using the port
$connections = netstat -ano | findstr ":$port"

if ($connections) {
    Write-Host "Found processes using port $port:" -ForegroundColor Yellow
    $connections
    
    # Extract PIDs
    $pids = @()
    $connections | ForEach-Object {
        $parts = $_ -split '\s+', 6
        if ($parts.Count -gt 4) {
            $pid = $parts[4]
            if ($pid -match '^\d+$') {
                $pids += $pid
            }
        }
    }
    
    $pids = $pids | Select-Object -Unique
    
    if ($pids.Count -gt 0) {
        Write-Host "Found PIDs: $($pids -join ', ')" -ForegroundColor Yellow
        
        # Show process info
        foreach ($pid in $pids) {
            try {
                $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
                if ($process) {
                    Write-Host "Process $pid is: $($process.Name) ($($process.Path))" -ForegroundColor Magenta
                }
            } catch {
                Write-Host "Could not get details for process $pid" -ForegroundColor Red
            }
        }
        
        # Ask if we should kill them
        $answer = Read-Host "Do you want to kill these processes? (Y/N)"
        if ($answer -eq "Y" -or $answer -eq "y") {
            foreach ($pid in $pids) {
                try {
                    Write-Host "Killing process $pid..." -ForegroundColor Red
                    Stop-Process -Id $pid -Force
                    Write-Host "Process $pid killed successfully." -ForegroundColor Green
                } catch {
                    Write-Host "Failed to kill process $pid. Try running as administrator." -ForegroundColor Red
                }
            }
        }
    }
} else {
    Write-Host "No processes found using port $port." -ForegroundColor Green
}

# Check for .NET processes
Write-Host "`nChecking for dotnet processes..." -ForegroundColor Cyan
$dotnetProcesses = Get-Process -Name dotnet -ErrorAction SilentlyContinue

if ($dotnetProcesses) {
    Write-Host "Found dotnet processes:" -ForegroundColor Yellow
    $dotnetProcesses | Format-Table Id, Name, Path, StartTime
    
    $answer = Read-Host "Do you want to kill all dotnet processes? (Y/N)"
    if ($answer -eq "Y" -or $answer -eq "y") {
        try {
            Write-Host "Killing all dotnet processes..." -ForegroundColor Red
            Stop-Process -Name dotnet -Force
            Write-Host "All dotnet processes killed successfully." -ForegroundColor Green
        } catch {
            Write-Host "Failed to kill some dotnet processes. Try running as administrator." -ForegroundColor Red
        }
    }
} else {
    Write-Host "No dotnet processes found." -ForegroundColor Green
}

# Check for VS processes that might be holding the port
Write-Host "`nChecking for Visual Studio processes..." -ForegroundColor Cyan
$vsProcesses = Get-Process -Name *devenv*, *ServiceHub* -ErrorAction SilentlyContinue

if ($vsProcesses) {
    Write-Host "Found Visual Studio processes that might be holding ports:" -ForegroundColor Yellow
    $vsProcesses | Format-Table Id, Name, Path, StartTime
    
    $answer = Read-Host "Do you want to kill these Visual Studio processes? (Y/N)"
    if ($answer -eq "Y" -or $answer -eq "y") {
        try {
            Write-Host "Killing Visual Studio processes..." -ForegroundColor Red
            $vsProcesses | ForEach-Object { Stop-Process -Id $_.Id -Force }
            Write-Host "Visual Studio processes killed successfully." -ForegroundColor Green
        } catch {
            Write-Host "Failed to kill some Visual Studio processes. Try running as administrator." -ForegroundColor Red
        }
    }
} else {
    Write-Host "No Visual Studio processes found." -ForegroundColor Green
}

Write-Host "`nTry running your application now:" -ForegroundColor Cyan
Write-Host "  dotnet run" -ForegroundColor Green
Write-Host "  # or with a custom port" -ForegroundColor Green
Write-Host "  `$env:APP_PORT = '9876'" -ForegroundColor Green
Write-Host "  dotnet run" -ForegroundColor Green
