# Phase 7: Authentication and Security

Phase 7 adds `SecureAspNetCoreApi`, a separate API that demonstrates ASP.NET
Core Identity, password hashing, JWT bearer authentication, roles, claims,
policies, CORS, rate limiting, HTTPS redirection, and security headers.

## 1. Identity and password hashing

ASP.NET Core Identity manages users, passwords, roles, claims, and related
security data through `UserManager<TUser>` and `RoleManager<TRole>`.

```csharp
IdentityResult result = await userManager.CreateAsync(user, password);
```

The application never stores the supplied password. Identity hashes it with a
password hasher and stores the resulting hash and security metadata. Login
checks the supplied password against that hash.

Password requirements are configured in the service registration. They are
only one part of password security; production applications should also add
email verification, password reset, lockout, breached-password controls, and
multi-factor authentication where appropriate.

## 2. JWT bearer authentication

The login endpoint creates a signed access token after Identity verifies the
password. The client sends the token in an HTTP header:

```text
Authorization: Bearer eyJ...
```

The JWT validation middleware checks:

- the signature;
- the issuer;
- the audience;
- the expiration time;
- the signing key.

The API does not use a cookie to carry authentication state. The bearer token
is sent explicitly in the `Authorization` header.

The demo generates an ephemeral signing key when `Jwt:Key` is absent so it can
start without a committed secret. Tokens become invalid after a process
restart. Production must provide a durable, high-entropy signing secret from a
secret manager or environment configuration. Never commit a real key to the
repository or place one in chat output.

## 3. Claims, roles, and policies

Authentication answers “who is this?” Authorization answers “what may this
identity do?”

The token includes identity claims and the user's Identity roles. The API
demonstrates three authorization forms:

```csharp
[Authorize]
public IActionResult Me() { ... }

[Authorize(Roles = "Admin")]
public IActionResult AdminOnly() { ... }

[Authorize(Policy = "CourseManagement")]
public IActionResult ManageCourses() { ... }
```

The `CourseManagement` policy requires the claim
`permission=course.manage`. A user can be authenticated and still receive
`403 Forbidden` because the required role or permission is missing.

The sample registration endpoint always assigns the least-privileged `User`
role. It never accepts a role from an untrusted registration request, because
that would allow a caller to grant themselves administrator access.

## 4. HTTP security results

- `200 OK`: the request succeeded.
- `201 Created`: a user was created.
- `400 Bad Request`: request validation failed.
- `401 Unauthorized`: no valid bearer token was supplied.
- `403 Forbidden`: the token is valid but lacks the required role or claim.
- `429 Too Many Requests`: a rate limit was exceeded.

The authentication and authorization middleware must run before protected
controller endpoints. `UseAuthentication()` builds the user principal;
`UseAuthorization()` evaluates `[Authorize]` metadata.

## 5. Rate limiting

Authentication endpoints are attractive targets for password guessing and
credential stuffing. The API applies a fixed-window limiter to registration
and login:

```text
5 authentication requests per minute in production
```

Real systems usually combine rate limits with account lockout, IP and device
signals, monitoring, and carefully designed recovery flows. A rate limiter is
one layer, not a complete abuse prevention system.

## 6. CORS

Cross-Origin Resource Sharing controls which browser origins may call the API.
The sample allows the configured frontend origin, defaults to
`http://localhost:3000`, and allows ordinary headers and methods.

Keep the allow-list explicit. Avoid `AllowAnyOrigin` for credentialed browser
flows, and do not treat CORS as authentication. Non-browser clients can call
an API regardless of browser CORS rules.

## 7. HTTPS and CSRF

The application redirects to HTTPS outside the test environment. Production
must also use valid certificates, secure proxy configuration, and secure
secret transport.

Bearer tokens sent in the `Authorization` header are not automatically sent by
the browser as ambient cookie credentials, which changes the usual CSRF threat
model. If an application uses cookie authentication, it must add antiforgery
tokens and appropriate SameSite and origin protections. Do not copy the bearer
token into a cookie without re-evaluating that design.

## 8. XSS, SQL injection, and input validation

- JSON serialization encodes string values as data. Do not render untrusted
  values as raw HTML.
- EF Core LINQ and parameterized APIs help prevent SQL injection. Never
  concatenate request values into SQL strings.
- DataAnnotations and Identity validation reject malformed input, but domain
  rules and database constraints remain necessary.
- Log security events without logging passwords, access tokens, or sensitive
  personal data.

The API adds `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, and a
strict `Referrer-Policy` as basic response hardening. Headers are one layer of
defense and should be reviewed together with deployment and browser policy.

## Run the phase

From the repository root:

```bash
dotnet run --project SecureAspNetCoreApi/SecureAspNetCoreApi.csproj
dotnet test CSharpCore.Tests/CSharpCore.Tests.csproj
```

The local API uses `secure-api.db`, which is ignored by the repository. Set a
production signing key through configuration, for example through a secret
manager or environment variable named `Jwt__Key`.

The next phase covers MVC, Razor syntax, Razor Pages, and the relationship
between Razor views and Razor components.
