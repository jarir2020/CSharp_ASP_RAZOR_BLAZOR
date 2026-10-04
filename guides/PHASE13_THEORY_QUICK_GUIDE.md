# Phase 13: Advanced web development

Phase 13 brings several web features together in AdvancedWebLab. The goal is
to see the boundary between an HTTP API, infrastructure around that API, and
long-lived real-time connections.

## 1. OpenAPI and Swagger

OpenAPI is a machine-readable description of an HTTP API. It describes routes,
parameters, request bodies, response shapes, and authentication requirements.

Swagger UI is a browser interface that reads an OpenAPI document and lets a
developer explore or call the documented endpoints.

This phase exposes a small hand-written contract at /openapi/v1.json and a
minimal /swagger page. Keeping the document visible makes the contract idea
clear. In a production project, an OpenAPI package can generate the document
from endpoint metadata and provide a full interactive UI. The contract still
needs review because generated documentation cannot decide whether a public
route is safe or whether its response shape is a good long-term API contract.

## 2. Rate limiting

Rate limiting controls how many requests a client can make during a time
window. AddFixedWindowLimiter gives this lesson a policy with two permits per
30 seconds:

~~~csharp
options.AddFixedWindowLimiter("course-demo", limiterOptions =>
{
    limiterOptions.PermitLimit = 2;
    limiterOptions.Window = TimeSpan.FromSeconds(30);
    limiterOptions.QueueLimit = 0;
});
~~~

The /api/v1/limited route attaches that named policy. A rejected request returns
HTTP 429 Too Many Requests. Real systems often partition limits by user, API
key, tenant, or IP address and choose a policy per endpoint class. The
partition key and limit should match the actual abuse and capacity model.

## 3. Health checks

Health checks answer operational questions for load balancers and deployment
systems. This lab has:

~~~text
/health        -> all registered checks
/health/ready  -> checks tagged ready
~~~

CourseCatalogHealthCheck reports healthy when the catalog has data. A real
application can add separate checks for database connectivity, queue
reachability, dependency credentials, and critical external services. Keep
liveness small: it should answer whether the process can run. Readiness can be
stricter and should answer whether this instance can receive traffic.

## 4. File uploads

The POST /api/v1/files endpoint expects a multipart form field named file. It
demonstrates a safe sequence:

~~~text
check content type
read the form
require a non-empty file
enforce a size limit
allow-list extensions
use Path.GetFileName for display metadata
consume the stream
~~~

The sample copies the stream to Stream.Null, so it does not create files on the
machine. A production upload flow also needs authorization, content inspection,
malware scanning, storage outside the web root, generated storage names, and a
download policy. An extension check is useful input validation, but it does
not prove that file bytes are safe.

## 5. Streaming

/api/v1/stream sends newline-delimited JSON (NDJSON). Each item is serialized,
written, and flushed as it becomes available:

~~~text
{"sequence":1,...}
{"sequence":2,...}
~~~

Streaming avoids building one large response in memory and lets a client start
processing before the whole operation finishes. The client must understand the
framing rule. NDJSON uses one JSON value per line; Server-Sent Events use event
fields; gRPC uses its own framed protocol.

## 6. Pagination, filtering, and sorting

The course endpoints accept:

~~~text
page=1&pageSize=3&search=core&sort=title
~~~

Pagination limits the response window. Filtering reduces the matching set.
Sorting gives a stable client-visible order. The catalog allow-lists sort names
and clamps page sizes. When moving to EF Core, apply Where, OrderBy, Skip, and
Take to the database query before materializing results. Add a stable
tie-breaker such as an ID so two pages do not shift unpredictably.

## 7. API versioning

This lesson uses explicit URL versions:

~~~text
/api/v1/courses
/api/v2/courses
~~~

Version 2 adds durationMinutes and returns an X-Api-Version response header.
The example is intentionally simple. A production versioning policy should
define how long old versions live, how clients migrate, and how breaking
changes are communicated. Headers or media types are also possible versioning
locations; consistency matters more than the particular location.

## 8. WebSockets

WebSockets upgrade an HTTP request into a bidirectional connection. The
/ws/echo endpoint accepts one message and sends it back. A browser client can
connect like this:

~~~javascript
const socket = new WebSocket("ws://localhost:5489/ws/echo");
socket.addEventListener("open", () => socket.send("hello"));
socket.addEventListener("message", event => console.log(event.data));
~~~

The sample keeps the protocol deliberately small. A real protocol needs message
types, authentication, maximum frame sizes, cancellation, heartbeats, and a
clear close strategy.

## 9. SignalR

SignalR provides a higher-level real-time programming model. The app registers
AddSignalR and maps CourseHub at /hubs/courses. Clients invoke hub methods and
receive named events such as CourseAnnouncement. SignalR can negotiate
transports and has a client library, while a raw WebSocket endpoint leaves the
message protocol entirely to the application.

The hub is not automatically an authorization boundary. Add authentication,
authorization policies, input validation, and group membership rules before
using a hub for private data.

## 10. gRPC

gRPC uses Protocol Buffers contracts and HTTP/2. Protos/course.proto defines the
service and messages. The companion guide in AdvancedWebLab/Grpc/README.md
shows the package references, generated server base class, AddGrpc, and
MapGrpcService needed in a package-backed gRPC project.

The main lab keeps that dependency separate so the other Phase 13 examples
remain easy to build with the shared ASP.NET Core framework. gRPC is a strong
choice for service-to-service calls where both sides can share generated
contracts. JSON HTTP APIs remain convenient for browsers, scripts, and broad
interoperability.

## Run and inspect the phase

~~~bash
dotnet run --project AdvancedWebLab/AdvancedWebLab.csproj
curl http://localhost:5489/openapi/v1.json
curl http://localhost:5489/health
curl 'http://localhost:5489/api/v1/courses?page=1&pageSize=2&sort=title'
curl http://localhost:5489/api/v1/limited
~~~

The integration tests verify the contract and health endpoints, versioned
catalog queries, uploads, NDJSON response, rate-limit rejection, and SignalR
negotiation. WebSocket echo is best exercised with a WebSocket-capable client.
