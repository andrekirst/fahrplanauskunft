# fahrplanauskunft
Kata of ccd school

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) and Docker Compose

## Development Setup

### 1. Database Setup

Start the PostgreSQL database using Docker Compose:

```bash
# Copy environment template and adjust if needed
cp .env.example .env

# Start PostgreSQL (use 'docker compose' or 'docker-compose')
docker compose up -d

# Verify PostgreSQL is running
docker compose ps
```

> **Note:** Use `docker compose` (v2, recommended) or `docker-compose` (v1, legacy). Both work with this configuration.

The database will be available at:
- **Host:** localhost
- **Port:** 5432
- **Database:** fahrplan
- **User:** fahrplan
- **Password:** fahrplan_dev (default, change in `.env` for production)

### 2. Test Database Connection

```bash
# Using psql (if installed)
psql -h localhost -p 5432 -U fahrplan -d fahrplan

# Or using Docker
docker exec -it fahrplanauskunft-postgres psql -U fahrplan -d fahrplan
```

### 3. Build and Run

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the CLI application
dotnet run --project src/Fahrplanauskunft.CLI
```

### 4. Run Tests

```bash
dotnet test
```

## Configuration

The CLI application uses the following configuration files:

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides

Connection string format:
```
Host=localhost;Port=5432;Database=fahrplan;Username=fahrplan;Password=fahrplan_dev
```

## Docker Commands

```bash
# Start database
docker compose up -d

# Stop database (preserves data)
docker compose stop

# Stop and remove containers (preserves data volume)
docker compose down

# Stop and remove everything including data
docker compose down -v

# View logs
docker compose logs -f postgres
```
