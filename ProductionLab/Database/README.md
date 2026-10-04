# Database migration exercise

The running sample keeps its audit buffer in memory so the project stays
package-free and easy to study. The SQL files in this directory demonstrate a
small migration history for a durable SQLite audit store.

Apply migrations in version order:

~~~bash
sqlite3 production-lab.db < ProductionLab/Database/001_create_audit_events.sql
sqlite3 production-lab.db < ProductionLab/Database/002_add_request_source.sql
~~~

Real migration systems record applied versions and run each migration once.
EF Core migrations, DbUp, FluentMigrator, and database-native migration tools
can provide that workflow. Review destructive changes, backup production data,
and make deployment order explicit before applying a schema change.
