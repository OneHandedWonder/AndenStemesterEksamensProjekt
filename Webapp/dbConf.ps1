# ============================================================================
# Database Initialization Script
# ============================================================================
# This script will:
# 1. Stop and remove existing database containers
# 2. Remove existing database volumes (WIPES ALL DATA!)
# 3. Start fresh PostgreSQL container via docker-compose
# 4. Wait for database to be ready
# 5. Execute all .sql files in dbInit/ folder
#
# Use -Reinit to only re-run SQL files without wiping the database
# ============================================================================

param(
    [switch]$Force,
    [switch]$Reinit
)

# Script configuration
$ErrorActionPreference = "Stop"
$composeFile = "docker-compose.yml"
$dbInitFolder = "dbInit"
$containerName = "EksamenSys_db"
$volumeName = "EksamenSys_db_data"
$dbUser = "appuser"
$dbPassword = "SuperSecurePassword123"
$dbName = "EksamenSys_db"
$dbPort = "5432"

# Color functions
function Write-Step {
    param([string]$Message)
    Write-Host "`n[STEP] $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Warning-Custom {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "  $Message" -ForegroundColor Gray
}

# ============================================================================
# Display banner
# ============================================================================
Clear-Host
Write-Host "============================================================================" -ForegroundColor Magenta
if ($Reinit) {
    Write-Host "           DATABASE RE-INITIALIZATION SCRIPT (SQL ONLY)" -ForegroundColor Magenta
} else {
    Write-Host "           DATABASE INITIALIZATION AND SETUP SCRIPT" -ForegroundColor Magenta
}
Write-Host "============================================================================" -ForegroundColor Magenta
Write-Host ""

# ============================================================================
# Pre-flight checks
# ============================================================================
Write-Step "Running pre-flight checks..."

# Check if Docker is running
try {
    docker ps | Out-Null
    Write-Success "Docker is running"
} catch {
    Write-Error-Custom "Docker is not running or not installed"
    Write-Info "Please start Docker Desktop and try again"
    exit 1
}

# Check if docker-compose.yml exists
if (-not (Test-Path $composeFile)) {
    Write-Error-Custom "docker-compose.yml not found in current directory"
    Write-Info "Current directory: $(Get-Location)"
    exit 1
}
Write-Success "Found docker-compose.yml"

# Check if dbInit folder exists
if (-not (Test-Path $dbInitFolder)) {
    Write-Warning-Custom "dbInit folder not found. Creating it..."
    New-Item -ItemType Directory -Path $dbInitFolder | Out-Null
    Write-Success "Created dbInit folder"
}

# Count SQL files
$sqlFiles = Get-ChildItem -Path $dbInitFolder -Filter "*.sql" | Sort-Object Name
$sqlFileCount = $sqlFiles.Count
Write-Info "Found $sqlFileCount SQL file(s) in $dbInitFolder folder"

# ============================================================================
# REINIT MODE - Skip to SQL execution
# ============================================================================
if ($Reinit) {
    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor Cyan
    Write-Host "                      REINIT MODE: SQL EXECUTION ONLY" -ForegroundColor Cyan
    Write-Host "============================================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "" -ForegroundColor Yellow
    Write-Host "  Skip container/volume cleanup" -ForegroundColor Yellow
    Write-Host "  Use the existing running database" -ForegroundColor Yellow
    Write-Host "  Execute all SQL scripts from dbInit folder" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  This will run SQL scripts against your existing database!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor Cyan
    Write-Host ""
    
    if (-not $Force) {
        $confirmation = Read-Host "Do you want to continue? Type 'YES' to proceed"
        if ($confirmation -ne "YES") {
            Write-Warning-Custom "Operation cancelled by user"
            exit 0
        }
    }
    
    # Check if container is running
    Write-Step "Checking if database container is running..."
    $containerRunning = docker ps --filter "name=$containerName" --format "{{.Names}}" 2>$null
    
    if (-not $containerRunning) {
        Write-Error-Custom "Container '$containerName' is not running"
        Write-Info "Start it with: docker compose up -d"
        exit 1
    }
    Write-Success "Container is running"
    
    # Skip to SQL execution section
    $skipToSqlExecution = $true
} else {
    # ============================================================================
    # WARNING AND CONFIRMATION
    # ============================================================================
    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor Red
    Write-Host "                             WARNING  " -ForegroundColor Red
    Write-Host "============================================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "This script will:" -ForegroundColor Yellow
    Write-Host "  Stop and remove the existing database container" -ForegroundColor Yellow
    Write-Host "  DELETE ALL DATA in the database volume" -ForegroundColor Yellow
    Write-Host "  Create a fresh PostgreSQL database" -ForegroundColor Yellow
    Write-Host "  Run all SQL scripts in the dbInit folder" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  ALL EXISTING DATA WILL BE PERMANENTLY LOST!" -ForegroundColor Red
    Write-Host ""
    Write-Host "============================================================================" -ForegroundColor Red
    Write-Host ""

    if (-not $Force) {
        $confirmation = Read-Host "Do you want to continue? Type 'YES' to proceed"
        if ($confirmation -ne "YES") {
            Write-Warning-Custom "Operation cancelled by user"
            exit 0
        }
    }
    
    $skipToSqlExecution = $false
}

if (-not $skipToSqlExecution) {
    # ============================================================================
    # Stop and remove existing containers
    # ============================================================================
    Write-Step "Stopping and removing existing containers..."

    try {
        # Check if container exists
        $containerExists = docker ps -a --filter "name=$containerName" --format "{{.Names}}" 2>$null
        
        if ($containerExists) {
            Write-Info "Stopping container: $containerName"
            docker stop $containerName 2>$null | Out-Null
            
            Write-Info "Removing container: $containerName"
            docker rm $containerName 2>$null | Out-Null
            
            Write-Success "Container removed successfully"
        } else {
            Write-Info "No existing container found"
        }
    } catch {
        Write-Warning-Custom "Could not stop/remove container: $($_.Exception.Message)"
    }

    # ============================================================================
    # Remove existing volumes
    # ============================================================================
    Write-Step "Removing database volumes..."

    try {
        # Check if volume exists
        $volumeExists = docker volume ls --filter "name=$volumeName" --format "{{.Name}}" 2>$null
        
        if ($volumeExists) {
            Write-Info "Removing volume: $volumeName"
            docker volume rm $volumeName 2>$null | Out-Null
            Write-Success "Volume removed successfully (all data wiped)"
        } else {
            Write-Info "No existing volume found"
        }
    } catch {
        Write-Warning-Custom "Could not remove volume: $($_.Exception.Message)"
    }

    # ============================================================================
    # Start Docker Compose
    # ============================================================================
    Write-Step "Starting PostgreSQL container via docker-compose..."

    try {
        docker compose up -d
        Write-Success "Docker Compose started successfully"
    } catch {
        Write-Error-Custom "Failed to start docker-compose: $($_.Exception.Message)"
        exit 1
    }

    # ============================================================================
    # Wait for database to be ready
    # ============================================================================
    Write-Step "Waiting for database to be ready..."

    $maxAttempts = 30
    $attempt = 0
    $isReady = $false

    while ($attempt -lt $maxAttempts -and -not $isReady) {
        $attempt++
        Write-Info "Checking database health (attempt $attempt/$maxAttempts)..."
        
        try {
            $healthStatus = docker inspect --format='{{.State.Health.Status}}' $containerName 2>$null
            
            if ($healthStatus -eq "healthy") {
                $isReady = $true
                Write-Success "Database is ready and healthy!"
            } else {
                Write-Info "Database status: $healthStatus (waiting...)"
                Start-Sleep -Seconds 2
            }
        } catch {
            Write-Info "Waiting for container to start..."
            Start-Sleep -Seconds 2
        }
    }

    if (-not $isReady) {
        Write-Error-Custom "Database did not become ready in time"
        Write-Info "Check logs with: docker logs $containerName"
        exit 1
    }

    # Give it a couple more seconds to be absolutely sure
    Start-Sleep -Seconds 3
}

# ============================================================================
# Execute SQL files
# ============================================================================
if ($sqlFileCount -gt 0) {
    Write-Step "Executing SQL files from $dbInitFolder folder..."
    Write-Info "NOTE: Files mounted to /docker-entrypoint-initdb.d are auto-executed on first start"
    Write-Info "If you need to manually run additional scripts, they will execute now..."
    
    $successCount = 0
    $failCount = 0
    $warningCount = 0
    
    foreach ($sqlFile in $sqlFiles) {
        Write-Host ""
        Write-Info "Executing: $($sqlFile.Name)"
        
        # Execute SQL file via docker exec
        $env:PGPASSWORD = $dbPassword
        
        # Use try/catch to handle PowerShell's error stream
        $prevErrorAction = $ErrorActionPreference
        $ErrorActionPreference = 'Continue'  # Don't treat stderr as terminating error
        
        $result = Get-Content $sqlFile.FullName | docker exec -i $containerName psql -U $dbUser -d $dbName 2>&1
        $exitCode = $LASTEXITCODE
        
        $ErrorActionPreference = $prevErrorAction
        
        # Convert result to string for analysis, handling ErrorRecords properly
        $resultString = ""
        if ($result) {
            foreach ($item in $result) {
                if ($item -is [System.Management.Automation.ErrorRecord]) {
                    $resultString += $item.Exception.Message + "`n"
                } else {
                    $resultString += $item.ToString() + "`n"
                }
            }
            $resultString = $resultString.Trim()
        }
        
        # Check for NOTICE messages (these go to stderr but aren't errors)
        $hasNotice = $resultString -match "NOTICE:"
        
        # Check for actual ERROR messages
        $hasError = ($resultString -match "ERROR:") -and -not ($resultString -match "NOTICE:")
        
        # Determine status based on content analysis
        if ($hasError) {
            # Real error occurred
            Write-Error-Custom "Failed to execute: $($sqlFile.Name)"
            Write-Info "Error: $resultString"
            $failCount++
        } elseif ($hasNotice) {
            # Only notices, treat as warning
            # Extract and clean up NOTICE message(s)
            $noticeLines = @()
            foreach ($line in ($resultString -split "`n")) {
                if ($line -match 'NOTICE:\s+(.+)') {
                    $noticeLines += $Matches[1]
                }
            }
            $noticeText = $noticeLines -join "; "
            Write-Warning-Custom "Executed with notices: $noticeText - $($sqlFile.Name)"
            $warningCount++
            $successCount++
        } else {
            # Clean success
            Write-Success "Successfully executed: $($sqlFile.Name)"
            $successCount++
        }
    }
    
    Write-Host ""
    Write-Host "SQL Execution Summary:" -ForegroundColor Cyan
    Write-Host "  Total files: $sqlFileCount" -ForegroundColor White
    Write-Host "  Successful:  $successCount" -ForegroundColor Green
    if ($warningCount -gt 0) {
        Write-Host "  Warnings:    $warningCount" -ForegroundColor Yellow
    }
    if ($failCount -gt 0) {
        Write-Host "  Failed:      $failCount" -ForegroundColor Red
    } else {
        Write-Host "  Failed:      $failCount" -ForegroundColor Green
    }
} else {
    Write-Warning-Custom "No SQL files found in $dbInitFolder folder"
    Write-Info "Add .sql files to $dbInitFolder to have them executed automatically"
}

# ============================================================================
# Final status and connection info
# ============================================================================
Write-Host ""
Write-Host "============================================================================" -ForegroundColor Green
Write-Host "                    DATABASE SETUP COMPLETED!" -ForegroundColor Green
Write-Host "============================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Connection Details:" -ForegroundColor Cyan
Write-Host "  Host:     localhost" -ForegroundColor White
Write-Host "  Port:     $dbPort" -ForegroundColor White
Write-Host "  Database: $dbName" -ForegroundColor White
Write-Host "  User:     $dbUser" -ForegroundColor White
Write-Host "  Password: $dbPassword" -ForegroundColor White
Write-Host ""
Write-Host "Useful Commands:" -ForegroundColor Cyan
Write-Host "  View logs:         docker logs $containerName -f" -ForegroundColor Gray
Write-Host "  Connect to DB:     docker exec -it $containerName psql -U $dbUser -d $dbName" -ForegroundColor Gray
Write-Host "  Stop container:    docker compose down" -ForegroundColor Gray
Write-Host "  Restart container: docker compose restart" -ForegroundColor Gray
Write-Host ""
Write-Host "============================================================================" -ForegroundColor Green
