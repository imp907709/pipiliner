# SatellitePipeline

A **.NET 9** prototype for processing satellite imagery per agricultural field. The repo folder is `ptojs`; the solution is **SatellitePipeline** — a small, in-memory proof-of-concept for a production-style async pipeline.

## Overview

The system processes satellite data for a field over a date range and produces three artifacts:

1. **NDVI index** (`.tif`)
2. **TIFF** composite
3. **Preview** image (`.webp`)

Artifacts are stored in **MinIO** (simulated via fake key paths). Processing is **async**, driven by commands, a **transactional outbox**, and a message bus (RabbitMQ pattern, in-memory).

**Requirements:** [.NET 9 SDK](https://dotnet.microsoft.com/download)

## Architecture

Clean architecture with five projects:

| Layer | Project | Role |
|-------|---------|------|
| Domain | `SatellitePipeline.Domain` | Entities, status constants |
| Application | `SatellitePipeline.Application` | Commands, handlers, orchestrator, outbox |
| Infrastructure | `SatellitePipeline.Infrastructure` | In-memory DB/bus, fake processors, wiring |
| API | `SatellitePipeline.Api` | HTTP endpoints |
| Worker | `SatellitePipeline.Worker` | Console demo / backfill job |

```
┌─────────────┐     ┌──────────────┐     ┌─────────────────┐
│  API/Worker │────▶│  CommandBus  │────▶│ Command Handlers│
└─────────────┘     └──────────────┘     └────────┬────────┘
                                                  │
                                                  ▼
                                         ┌────────────────┐
                                         │ Outbox (DB)    │
                                         └────────┬───────┘
                                                  │
                                         OutboxDispatcher
                                                  │
                                                  ▼
                                         ┌────────────────┐
                                         │ InMemory MQ Bus│
                                         └────────┬───────┘
                    ┌─────────────────────────────┼─────────────────────────────┐
                    ▼                             ▼                             ▼
           Orchestrator                   IndexWorker                    TiffWorker / PreviewWorker
         (state machine)              (fake NDVI proc)              (fake TIFF/preview proc)
```

## Processing Flow

Steps: `start` → `create-index` → `create-tiff` → `create-preview` → `completed`

1. **Start** — create a `SatelliteProcessingRun`, enqueue `SatelliteProcessingRequested`
2. **Create index** — worker generates NDVI artifact, emits `IndexReady`
3. **Create TIFF** — worker generates TIFF artifact, emits `TiffReady`
4. **Create preview** — worker generates preview artifact, emits `PreviewReady`
5. **Complete** — run marked completed

`SatelliteProcessOrchestrator` chains **ready** events to the next **request** command. Workers handle **create** requests and emit **complete** commands. Commands mutate state and write to the outbox; `OutboxDispatcher` publishes pending messages to the bus.

## Entry Points

**API** — minimal HTTP surface:

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/satellite-processing` | Start a run (`202 Accepted`; does not return run ID) |
| `POST` | `/outbox/drain` | Publish pending outbox messages |
| `GET` | `/runs` | Inspect runs, artifacts, and outbox state |

**Worker** — runs a hardcoded backfill job (Brazil soy field), drains the outbox in a loop, prints results to console. Simulates a Quartz-style nightly job.

## Design Patterns

- **CQRS** — commands with dedicated handlers via `CommandBus`
- **Transactional outbox** — messages persisted before dispatch
- **Saga / orchestration** — `SatelliteProcessOrchestrator` advances the pipeline on events
- **Idempotency** — duplicate runs skipped by `(fieldId, geometryVersion, periodFrom, periodTo)`; workers skip if artifact already exists
- **Correlation IDs** — propagated across commands and messages

## Domain Model

- **`SatelliteProcessingRun`** — status (`pending` / `running` / `completed` / `failed`) and current step
- **`SatelliteArtifact`** — index / tiff / preview with MinIO key and checksum
- **`OutboxMessage`** — `pending` / `published` / `failed`, with retry count

## Current State

Everything is **in-memory** — no real persistence or messaging:

| Component | Implementation |
|-----------|----------------|
| Database | `InMemoryAppDb` (lists in memory) |
| Message bus | `InMemoryRabbitMqBus` (sync in-process handlers) |
| Processors | `FakeIndexProcessor`, `FakeTiffProcessor`, `FakePreviewProcessor` |
| Storage | Fake MinIO key paths only |

No NuGet dependencies beyond the .NET SDK. No solution file, tests, Docker, or EF Core.

## Gaps / Risks

1. **No shared persistence** — API and Worker each create their own `PipelineComposition`; state is not shared across processes.
2. **Synchronous bus** — handlers run inline during `DrainOnce`, so the full pipeline can complete in one drain cycle.
3. **No background outbox worker** — draining is manual (API endpoint or Worker loop).
4. **No failure handling in orchestrator** — `run.Fail()` exists but is never called; failed outbox messages are not retried.
5. **Duplicated handler logic** — `AddArtifactIfMissing` is copy-pasted across complete handlers.
6. **No validation or auth** on API inputs.
7. **API returns no run ID** — caller cannot correlate the accepted request with a run.

## Run

```bash
dotnet run --project src/SatellitePipeline.Worker   # console demo
dotnet run --project src/SatellitePipeline.Api      # HTTP API
```

## Summary

A **well-structured architectural skeleton** for a satellite imagery pipeline: clean layers, command/outbox/event-driven flow, and idempotent steps. Intended as a spike or reference implementation to be swapped for real PostgreSQL, RabbitMQ, MinIO, and satellite processors.
