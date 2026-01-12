# Docker Development Environment

This folder contains Docker configuration for local development.

## Files

- `docker-compose.dev.yml` - Docker Compose configuration for PostgreSQL
- `init.sql` - Seed data for development (test tenants and users)

## Quick Start

### 1. Start the database

```bash
cd docker
docker-compose -f docker-compose.dev.yml up -d
```

### 2. Verify it's running

```bash
docker ps
```

### 3. Connect to the database

```bash
docker exec -it portal-postgres psql -U postgres -d portal
```

## Connection Details

| Property | Value |
|----------|-------|
| Host | `localhost` |
| Port | `5433` |
| Database | `portal` |
| Username | `postgres` |
| Password | `postgres` |

Connection string:
```
postgresql://postgres:postgres@localhost:5433/portal
```

## Seed Data

The `init.sql` file automatically runs on first container creation and includes:

### Test Tenants

| Tenant | Slug | Tier |
|--------|------|------|
| Acme Corporation | `acme` | professional |
| Globex Industries | `globex` | starter |

### Test Users

| Email | Password | Tenant | Role |
|-------|----------|--------|------|
| admin@acme.com | `password123` | Acme | Admin |
| member@acme.com | `password123` | Acme | Member |
| admin@globex.com | `password123` | Globex | Admin |

## Common Commands

### Stop the database
```bash
docker-compose -f docker-compose.dev.yml down
```

### Reset database (delete all data and re-run init.sql)
```bash
docker-compose -f docker-compose.dev.yml down -v
docker-compose -f docker-compose.dev.yml up -d
```

### View logs
```bash
docker logs portal-postgres
```

### List databases
```bash
docker exec -it portal-postgres psql -U postgres -c "\l"
```

### Re-import init.sql manually (without resetting)
```bash
docker exec -i portal-postgres psql -U postgres -d portal < init.sql
```

## Troubleshooting

### init.sql not running
The init script only runs when the data volume is empty (first run). To re-run it:
1. Stop and remove volumes: `docker-compose -f docker-compose.dev.yml down -v`
2. Start again: `docker-compose -f docker-compose.dev.yml up -d`

### Port conflict
If port 5433 is in use, edit `docker-compose.dev.yml` and change the port mapping.

### Permission denied
Ensure Docker is running and you have permission to use it.


Import data
docker exec -i portal-postgres psql -U postgres -d portal < init.sql