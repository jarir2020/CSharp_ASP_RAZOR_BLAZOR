-- Educational idempotent migration for a durable audit store.
-- The current sample uses an in-memory AuditStore; this script shows the
-- database shape and migration bookkeeping a production worker would apply.

CREATE TABLE IF NOT EXISTS schema_migrations (
    version TEXT PRIMARY KEY,
    applied_at_utc TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS audit_events (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    created_at_utc TEXT NOT NULL,
    correlation_id TEXT NOT NULL,
    method TEXT NOT NULL,
    path TEXT NOT NULL,
    status_code INTEGER NOT NULL,
    elapsed_milliseconds INTEGER NOT NULL
);

INSERT OR IGNORE INTO schema_migrations (version, applied_at_utc)
VALUES ('001_create_audit_events', CURRENT_TIMESTAMP);
