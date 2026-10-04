# Phase 15: Production and DevOps

Phase 15 turns an ASP.NET Core application into a deployable service. The
ProductionLab sample shows configuration validation, secret-safe diagnostics,
structured logging, correlation IDs, health endpoints, publishing, a
container image, reverse-proxy configuration, database migration scripts, and
CI/CD workflow structure.

## 1. Environments and configuration

Configuration is layered. A typical production application reads:

~~~text
appsettings.json
appsettings.Production.json
environment variables
command-line arguments
secret manager or external secret provider
~~~

ProductionLab binds the Production section to ProductionOptions and validates
it during startup. Environment variables use double underscores to represent
sections:

~~~text
Production__ServiceName=Course Platform API
Production__ExternalApiKey=provided-by-a-secret-manager
~~~

The application reports whether the secret is configured at
/api/config/safe, but never returns the secret value. A startup validation
failure is useful because the service fails before receiving traffic with an
incomplete production configuration.

Keep local secret files out of source control. The repository ignores .env and
database files; use a real secret manager or deployment secret store for
production credentials.

## 2. Kestrel and graceful hosting

Kestrel is the cross-platform ASP.NET Core web server. The container and
systemd examples set Kestrel to port 8080 through environment configuration,
which works well behind a reverse proxy. The proxy can own the public TLS
certificate while Kestrel serves the application network. Keeping the bind
address outside appsettings.json also lets deployment and command-line URL
settings override it predictably.

The application receives graceful shutdown signals from the host. Long-running
work should observe cancellation tokens, finish or stop safely, and avoid
leaving partially written data. The ShutdownTimeoutSeconds option is included
as a configuration lesson; a real host should apply its value to the host
lifetime settings as part of its deployment policy.

The /api/runtime endpoint is a diagnostic example. It exposes framework,
operating system, process ID, start time, and uptime, but it should be reviewed
before being made public on a sensitive service.

## 3. Logging and monitoring

ProductionLab enables JSON console logging and adds a correlation ID to each
request. The request middleware records method, path, status, elapsed time, and
correlation ID in a bounded in-memory diagnostic store.

Logs are event data. Prefer:

~~~csharp
logger.LogInformation(
    "HTTP {Method} {Path} returned {StatusCode}",
    method,
    path,
    statusCode);
~~~

Structured fields can be searched by a log collector. Do not log passwords,
access tokens, API keys, or unnecessary personal data. The in-memory audit
buffer is for the lesson; production audit data needs a durable, access
controlled store and a retention policy.

Monitoring normally combines logs, metrics, traces, and alerts. A useful alert
has an owner and an action, such as sustained readiness failure or a rising
rate of server errors.

## 4. Liveness and readiness

ProductionLab exposes:

~~~text
/health/live  -> the process can answer a basic health request
/health/ready -> required configuration is ready for traffic
~~~

Keep liveness independent from optional dependencies so an orchestrator can
distinguish a running process from a process ready to serve. Readiness can
check databases, queues, and external services when those dependencies are
required for requests.

Health endpoints should be protected from accidental caching and should avoid
returning secrets or detailed dependency credentials.

## 5. Publishing

A framework-dependent publish produces a deployment directory for a compatible
runtime:

~~~bash
dotnet publish ProductionLab/ProductionLab.csproj \
  --configuration Release \
  --output artifacts/production-lab
~~~

The included scripts/publish-production-lab.sh wraps that command. Publishing
compiles and gathers application assets; it does not perform a release by
itself. Deployment still needs a target directory, service configuration,
environment variables, migrations, health verification, and rollback plan.

## 6. Linux hosting

Hosting/production-lab.service is a systemd unit example. It runs the published
application as www-data, restarts after failure, and binds Kestrel to loopback.
A typical Linux handoff is:

~~~text
publish artifact
copy artifact to a versioned release directory
set ownership and permissions
install or update the systemd unit
inject environment secrets
apply database migrations
restart the service
check /health/ready
~~~

Use a separate release directory or symlink strategy when atomic rollback
matters. Do not copy a developer .env file into a public release directory.

## 7. Windows and IIS

Hosting/web.config shows the ASP.NET Core Module configuration used by IIS.
IIS acts as the Windows process manager and reverse proxy while the ASP.NET
Core Module starts the application.

The deployment machine needs the matching .NET Hosting Bundle. Configure
application pool identity, filesystem permissions, stdout logging only during
diagnosis, HTTPS bindings, and IIS request limits according to the application
contract. Windows hosting cannot be executed on this Linux machine, so the
repository contains the configuration and the guide explains the handoff.

## 8. Nginx and reverse proxy headers

Hosting/nginx-production-lab.conf forwards HTTP requests and WebSocket upgrade
headers to Kestrel. It also sends X-Forwarded-For and X-Forwarded-Proto.

ProductionLab enables forwarded headers and trusts loopback. A real deployment
must set KnownProxies or KnownNetworks to the actual proxy addresses. Do not
trust arbitrary forwarded headers from the public internet. The public Nginx
server should terminate HTTPS, redirect HTTP to HTTPS, and keep the Kestrel
port private.

WebSocket and SignalR applications need HTTP/1.1 upgrade forwarding. The Nginx
map in the sample preserves that connection behavior.

## 9. Docker and Compose

The multi-stage ProductionLab/Dockerfile uses an SDK image for compilation and
an ASP.NET Core runtime image for execution. The final image contains the
published application rather than the SDK. It also uses the non-root APP_UID
provided by the official image.

The root docker-compose.yml maps port 8080 and reads
Production__ExternalApiKey from the host environment. Run it only after setting
the variable in the shell or a protected deployment environment:

~~~bash
export Production__ExternalApiKey='value-from-secret-store'
docker compose up --build
~~~

Never commit that export, a real .env file, or a credential in a Compose file.
The Docker daemon on this machine is currently permission-restricted, so image
execution needs Docker socket access outside the current sandbox.

## 10. Database migrations

Database/001_create_audit_events.sql and 002_add_request_source.sql demonstrate
ordered, idempotent migration files and a schema_migrations table. Apply them
in order:

~~~bash
sqlite3 production-lab.db < ProductionLab/Database/001_create_audit_events.sql
sqlite3 production-lab.db < ProductionLab/Database/002_add_request_source.sql
~~~

A production migration process should use a backup, a tested upgrade path,
locking or coordination, observability, and a rollback or forward-fix plan.
Test migrations against a copy of production-like data before release.

## 11. HTTPS

Local development can use the .NET development certificate. A public
production deployment should use a certificate issued and renewed by the
organization's certificate or cloud provider. Nginx or IIS can terminate TLS,
then forward the trusted scheme to Kestrel.

For a local self-signed exercise, OpenSSL can create a certificate, but clients
will need an explicit trust decision. Never treat an untrusted development
certificate as a production certificate.

## 12. CI/CD

.github/workflows/production-lab.yml demonstrates a basic pipeline:

~~~text
checkout
install .NET 10
restore
build
test
publish
upload artifact
~~~

CI proves that source can build and tests can run. A release pipeline should
add artifact promotion, environment approval, secret injection, database
migration coordination, deployment, health verification, and rollback.

## What this machine can validate

This Linux machine has Docker and Docker Compose clients, curl, OpenSSL, and a
local .NET 9 SDK. Nginx and IIS are not installed, and the Docker client cannot
currently access its daemon. The project targets .NET 10, so local source
validation uses the available SDK's net9 target override. CI is configured for
the intended .NET 10 build.

## Run the phase

~~~bash
/home/jarir-ahmed/.dotnet/dotnet restore ProductionLab/ProductionLab.csproj \
  -p:TargetFramework=net9.0
/home/jarir-ahmed/.dotnet/dotnet build ProductionLab/ProductionLab.csproj \
  -p:TargetFramework=net9.0 --no-restore
~~~

Run the HTTP application in the Testing environment for a local smoke test:

~~~bash
/home/jarir-ahmed/.dotnet/dotnet run \
  --project ProductionLab/ProductionLab.csproj \
  -p:TargetFramework=net9.0 \
  --no-restore \
  --no-build \
  --no-launch-profile \
  --urls http://127.0.0.1:8104 \
  -- --environment Testing
~~~
