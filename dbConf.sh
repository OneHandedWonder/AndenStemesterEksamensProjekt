#!/bin/bash

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
# Use --reinit to only re-run SQL files without wiping the database
# ============================================================================

set -e

# Script configuration
COMPOSE_FILE="docker-compose.yml"
DB_INIT_FOLDER="dbInit"
CONTAINER_NAME="EksamenSys_db"
VOLUME_NAME="EksamenSys_db_data"
DB_USER="appuser"
DB_PASSWORD="SuperSecurePassword123"
DB_NAME="EksamenSys_db"
DB_PORT="5432"

# Parse arguments
FORCE=false
REINIT=false
while [[ "$#" -gt 0 ]]; do
    case $1 in
        --force|-f) FORCE=true ;;
        --reinit|-r) REINIT=true ;;
        *) echo "Unknown parameter: $1"; exit 1 ;;
    esac
    shift
done

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
GRAY='\033[0;37m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

# Helper functions
print_step() {
    echo -e "\n${CYAN}[STEP] $1${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "${GRAY}  $1${NC}"
}

# ============================================================================
# Display banner
# ============================================================================
clear
echo -e "${MAGENTA}============================================================================${NC}"
if [ "$REINIT" = true ]; then
    echo -e "${MAGENTA}           DATABASE RE-INITIALIZATION SCRIPT (SQL ONLY)${NC}"
else
    echo -e "${MAGENTA}           DATABASE INITIALIZATION AND SETUP SCRIPT${NC}"
fi
echo -e "${MAGENTA}============================================================================${NC}"
echo ""

# ============================================================================
# Pre-flight checks
# ============================================================================
print_step "Running pre-flight checks..."

# Check if Docker is running
if ! docker ps &> /dev/null; then
    print_error "Docker is not running or not installed"
    print_info "Please start Docker and try again"
    exit 1
fi
print_success "Docker is running"

# Check if docker-compose.yml exists
if [ ! -f "$COMPOSE_FILE" ]; then
    print_error "docker-compose.yml not found in current directory"
    print_info "Current directory: $(pwd)"
    exit 1
fi
print_success "Found docker-compose.yml"

# Check if dbInit folder exists
if [ ! -d "$DB_INIT_FOLDER" ]; then
    print_warning "dbInit folder not found. Creating it..."
    mkdir -p "$DB_INIT_FOLDER"
    print_success "Created dbInit folder"
fi

# Count SQL files
SQL_FILE_COUNT=$(find "$DB_INIT_FOLDER" -maxdepth 1 -name "*.sql" -type f 2>/dev/null | wc -l)
print_info "Found $SQL_FILE_COUNT SQL file(s) in $DB_INIT_FOLDER folder"

# ============================================================================
# REINIT MODE - Skip to SQL execution
# ============================================================================
if [ "$REINIT" = true ]; then
    echo ""
    echo -e "${CYAN}============================================================================${NC}"
    echo -e "${CYAN}                      REINIT MODE: SQL EXECUTION ONLY${NC}"
    echo -e "${CYAN}============================================================================${NC}"
    echo ""
    echo -e "${YELLOW}This will:${NC}"
    echo -e "${YELLOW}  • Skip container/volume cleanup${NC}"
    echo -e "${YELLOW}  • Use the existing running database${NC}"
    echo -e "${YELLOW}  • Execute all SQL scripts from dbInit folder${NC}"
    echo ""
    echo -e "${YELLOW}⚠️  This will run SQL scripts against your existing database!${NC}"
    echo ""
    echo -e "${CYAN}============================================================================${NC}"
    echo ""
    
    if [ "$FORCE" = false ]; then
        read -p "Do you want to continue? Type 'YES' to proceed: " confirmation
        if [ "$confirmation" != "YES" ]; then
            print_warning "Operation cancelled by user"
            exit 0
        fi
    fi
    
    # Check if container is running
    print_step "Checking if database container is running..."
    if ! docker ps --filter "name=$CONTAINER_NAME" --format "{{.Names}}" | grep -q "^${CONTAINER_NAME}$"; then
        print_error "Container '$CONTAINER_NAME' is not running"
        print_info "Start it with: docker compose up -d"
        exit 1
    fi
    print_success "Container is running"
    
    # Skip to SQL execution section
    SKIP_TO_SQL=true
else
    # ============================================================================
    # WARNING AND CONFIRMATION
    # ============================================================================
    echo ""
    echo -e "${RED}============================================================================${NC}"
    echo -e "${RED}                           ⚠️  WARNING  ⚠️${NC}"
    echo -e "${RED}============================================================================${NC}"
    echo ""
    echo -e "${YELLOW}This script will:${NC}"
    echo -e "${YELLOW}  • Stop and remove the existing database container${NC}"
    echo -e "${YELLOW}  • DELETE ALL DATA in the database volume${NC}"
    echo -e "${YELLOW}  • Create a fresh PostgreSQL database${NC}"
    echo -e "${YELLOW}  • Run all SQL scripts in the dbInit folder${NC}"
    echo ""
    echo -e "${RED}⚠️  ALL EXISTING DATA WILL BE PERMANENTLY LOST!${NC}"
    echo ""
    echo -e "${RED}============================================================================${NC}"
    echo ""

    if [ "$FORCE" = false ]; then
        read -p "Do you want to continue? Type 'YES' to proceed: " confirmation
        if [ "$confirmation" != "YES" ]; then
            print_warning "Operation cancelled by user"
            exit 0
        fi
    fi
    
    SKIP_TO_SQL=false
fi

if [ "$SKIP_TO_SQL" = false ]; then
    # ============================================================================
    # Stop and remove existing containers
    # ============================================================================
    print_step "Stopping and removing existing containers..."

    # Check if container exists
    if docker ps -a --filter "name=$CONTAINER_NAME" --format "{{.Names}}" | grep -q "^${CONTAINER_NAME}$"; then
        print_info "Stopping container: $CONTAINER_NAME"
        docker stop "$CONTAINER_NAME" &> /dev/null || true
        
        print_info "Removing container: $CONTAINER_NAME"
        docker rm "$CONTAINER_NAME" &> /dev/null || true
        
        print_success "Container removed successfully"
    else
        print_info "No existing container found"
    fi

    # ============================================================================
    # Remove existing volumes
    # ============================================================================
    print_step "Removing database volumes..."

    # Check if volume exists
    if docker volume ls --filter "name=$VOLUME_NAME" --format "{{.Name}}" | grep -q "^${VOLUME_NAME}$"; then
        print_info "Removing volume: $VOLUME_NAME"
        docker volume rm "$VOLUME_NAME" &> /dev/null || true
        print_success "Volume removed successfully (all data wiped)"
    else
        print_info "No existing volume found"
    fi

    # ============================================================================
    # Start Docker Compose
    # ============================================================================
    print_step "Starting PostgreSQL container via docker-compose..."

    if docker compose up -d 2>/dev/null || docker-compose up -d 2>/dev/null; then
        print_success "Docker Compose started successfully"
    else
        print_error "Failed to start docker-compose"
        exit 1
    fi

    # ============================================================================
    # Wait for database to be ready
    # ============================================================================
    print_step "Waiting for database to be ready..."

    MAX_ATTEMPTS=30
    ATTEMPT=0
    IS_READY=false

    while [ $ATTEMPT -lt $MAX_ATTEMPTS ] && [ "$IS_READY" = false ]; do
        ATTEMPT=$((ATTEMPT + 1))
        print_info "Checking database health (attempt $ATTEMPT/$MAX_ATTEMPTS)..."
        
        HEALTH_STATUS=$(docker inspect --format='{{.State.Health.Status}}' "$CONTAINER_NAME" 2>/dev/null || echo "starting")
        
        if [ "$HEALTH_STATUS" = "healthy" ]; then
            IS_READY=true
            print_success "Database is ready and healthy!"
        else
            print_info "Database status: $HEALTH_STATUS (waiting...)"
            sleep 2
        fi
    done

    if [ "$IS_READY" = false ]; then
        print_error "Database did not become ready in time"
        print_info "Check logs with: docker logs $CONTAINER_NAME"
        exit 1
    fi

    # Give it a couple more seconds to be absolutely sure
    sleep 3
fi

# ============================================================================
# Execute SQL files
# ============================================================================
if [ "$SQL_FILE_COUNT" -gt 0 ]; then
    print_step "Executing SQL files from $DB_INIT_FOLDER folder..."
    print_info "NOTE: Files mounted to /docker-entrypoint-initdb.d are auto-executed on first start"
    print_info "If you need to manually run additional scripts, they will execute now..."
    
    SUCCESS_COUNT=0
    FAIL_COUNT=0
    
    # Process SQL files in alphabetical order
    while IFS= read -r -d '' sql_file; do
        echo ""
        FILENAME=$(basename "$sql_file")
        print_info "Executing: $FILENAME"
        
        if docker exec -i "$CONTAINER_NAME" psql -U "$DB_USER" -d "$DB_NAME" < "$sql_file" &> /dev/null; then
            print_success "Successfully executed: $FILENAME"
            SUCCESS_COUNT=$((SUCCESS_COUNT + 1))
        else
            print_error "Failed to execute: $FILENAME"
            FAIL_COUNT=$((FAIL_COUNT + 1))
        fi
    done < <(find "$DB_INIT_FOLDER" -maxdepth 1 -name "*.sql" -type f -print0 | sort -z)
    
    echo ""
    echo -e "${GRAY}─────────────────────────────────────────────────────────────────────────${NC}"
    echo -e "${CYAN}SQL Execution Summary:${NC}"
    echo -e "${WHITE}  Total files: $SQL_FILE_COUNT${NC}"
    echo -e "${GREEN}  Successful:  $SUCCESS_COUNT${NC}"
    if [ $FAIL_COUNT -gt 0 ]; then
        echo -e "${RED}  Failed:      $FAIL_COUNT${NC}"
    else
        echo -e "${GREEN}  Failed:      $FAIL_COUNT${NC}"
    fi
    echo -e "${GRAY}─────────────────────────────────────────────────────────────────────────${NC}"
else
    print_warning "No SQL files found in $DB_INIT_FOLDER folder"
    print_info "Add .sql files to $DB_INIT_FOLDER to have them executed automatically"
fi

# ============================================================================
# Final status and connection info
# ============================================================================
echo ""
echo -e "${GREEN}============================================================================${NC}"
echo -e "${GREEN}                    DATABASE SETUP COMPLETED!${NC}"
echo -e "${GREEN}============================================================================${NC}"
echo ""
echo -e "${CYAN}Connection Details:${NC}"
echo -e "${WHITE}  Host:     localhost${NC}"
echo -e "${WHITE}  Port:     $DB_PORT${NC}"
echo -e "${WHITE}  Database: $DB_NAME${NC}"
echo -e "${WHITE}  User:     $DB_USER${NC}"
echo -e "${WHITE}  Password: $DB_PASSWORD${NC}"
echo ""
echo -e "${CYAN}Useful Commands:${NC}"
echo -e "${GRAY}  View logs:         docker logs $CONTAINER_NAME -f${NC}"
echo -e "${GRAY}  Connect to DB:     docker exec -it $CONTAINER_NAME psql -U $DB_USER -d $DB_NAME${NC}"
echo -e "${GRAY}  Stop container:    docker compose down${NC}"
echo -e "${GRAY}  Restart container: docker compose restart${NC}"
echo ""
echo -e "${GREEN}============================================================================${NC}"
