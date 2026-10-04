-- A second migration demonstrates an additive, repeatable schema change.

CREATE INDEX IF NOT EXISTS idx_audit_events_correlation_id
ON audit_events (correlation_id);

INSERT OR IGNORE INTO schema_migrations (version, applied_at_utc)
VALUES ('002_add_request_source', CURRENT_TIMESTAMP);
