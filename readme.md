# JobRunner

A production-style background job processing system built in .NET that demonstrates safe concurrent execution, retries with exponential backoff, external API integration, and clean architectural separation.


---

## What It Does

- Safely processes background jobs with multiple workers
- Guarantees single acquisition (no double-processing)
- Retries failed jobs using exponential backoff
- Dispatches work by job type via pluggable handlers
- Persists attempts and external API results
- Exposes read-only APIs for operational visibility

---

## Architecture

JobRunner.Domain          // Core entities and abstractions  
JobRunner.Infrastructure // EF Core, queue, dispatcher, stores  
JobRunner                // ASP.NET Core API + background worker  

Key design choices:
- Domain is infrastructure-agnostic
- Atomic job acquisition via set-based updates
- Optimistic concurrency for correctness
- Handlers resolved by job type

---

## Core Concepts

- Job – Defines a runnable job type and retry policy
- JobRun – A single execution instance with lifecycle state
- JobAttempt – Tracks individual execution attempts
- ExternalApiSyncResult – Persists external API diagnostics

---

## Worker Flow

1. Dequeue eligible job
2. Atomically acquire it
3. Create attempt record
4. Dispatch handler by job type
5. Mark success or schedule retry

Retry Backoff:  
min(300s, 5 * 2^(attempt - 1))

---

## API

Enqueue Job  
POST /api/job-runs/enqueue

Get Job Run Details  
GET /api/job-runs/{id}

List Job Runs  
GET /api/job-runs?status=Queued&limit=50

---

## Engineering Focus

- Background worker + scoped DI correctness
- Atomic concurrency control
- Retry scheduling and auditability
- External integration with persisted results
- Extensible handler-based execution model

This is a service-oriented system, not CRUD.

---

## Disclaimer

This project is an independent learning and demonstration project.
