# Docker

## Host ports (+1000 offset)

Satellite pipeline infra uses **non-default host ports** so it can run beside other local stacks (e.g. `api-backend-infra` on 5432).

| Service | Default | **Satellite (host)** | Inside Docker network |
|---------|---------|----------------------|------------------------|
| PostgreSQL | 5432 | **6432** | `postgres:5432` |
| RabbitMQ AMQP | 5672 | **6672** | `rabbitmq:5672` |
| RabbitMQ UI | 15672 | **16672** | — |
| MinIO API | 9000 | **9100** | `minio:9000` |
| MinIO console | 9001 | **9101** | — |
| API (docker) | 6080 | **6180** | `api:6080` |

**Local debug** (apps on host): use **host** ports in `appsettings.Development.json` / `prePipeline/.env`.

**Containers talking to each other**: use service names + **internal** ports (unchanged).

### Config audit (host vs internal)

| File | Postgres | RabbitMQ | MinIO |
|------|----------|----------|-------|
| `docker-compose.infra.yml` | host **6432** | host **6672** / **16672** | host **9100** / **9101** |
| `appsettings.Development.json` (Api, Worker) | **6432** | **6672** | **9100** |
| `prePipeline/.env` | **6432** | **6672** | **9100** |
| `AppDbContextFactory.cs` / `alembic.ini` | **6432** | — | — |
| `appsettings.Docker.json` | `postgres:5432` | `rabbitmq:5672` | `minio:9000` |
| `docker-compose.yml` env | internal only | internal only | internal only |

Do **not** point host apps at `localhost:5432` or `localhost:5672` — that is your other stack.

## Infrastructure

PostgreSQL, RabbitMQ, and MinIO (object storage for artifacts):

```bash
docker compose -f docker-compose.infra.yml up -d
```

| Service   | URL / port                         | Credentials        |
|-----------|------------------------------------|--------------------|
| PostgreSQL | `localhost:6432`                  | `pipeline` / `pipeline` / DB `satellite_pipeline` |
| RabbitMQ  | AMQP `localhost:6672`, UI `http://localhost:16672` | `pipeline` / `pipeline` |
| MinIO     | API `http://localhost:9100`, console `http://localhost:9101` | `pipeline` / `pipeline123` |

Creates network `satellite-pipeline`.

## Applications (on infrastructure)

Start infra first, then:

```bash
docker compose up -d --build
```

| Service | URL |
|---------|-----|
| API | `http://localhost:6180` |

`docker-compose.yml` expects the external network from infra compose.

## All-in-one

```bash
docker compose -f docker-compose.full.yml up -d --build
```

## Local (no Docker)

Without `Infrastructure` connection settings, apps use in-memory DB and in-memory messaging:

```bash
dotnet run --project src/SatellitePipeline.Worker
dotnet run --project src/SatellitePipeline.Api
```

## Worker options

| Variable | Default | Description |
|----------|---------|-------------|
| `RUN_BACKFILL_ON_STARTUP` | `false` | Run demo backfill job on worker start |

## API endpoints (Docker)

- `GET /health` — readiness + infra flags
- `POST /satellite-processing` — start run
- `POST /outbox/drain` — manual outbox drain (also runs on timer)
- `GET /runs` — inspect state
