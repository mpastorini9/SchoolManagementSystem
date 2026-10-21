# Authentication

## Purpose

This document defines the authentication strategy used by the application's API.

Authentication establishes the identity of the caller before any protected operation can be performed.

Its purpose is to ensure that every request can be associated with a verified identity while keeping authentication independent from business logic.

---

# Authentication Philosophy

Authentication answers one question:

**Who is making this request?**

It establishes identity.

It does not determine permissions, business rules, or application behavior.

Authentication should be treated as an infrastructure concern rather than a business concern.

---

# Authentication vs Authorization

Authentication and authorization are different responsibilities.

Authentication verifies identity.

Authorization determines what an authenticated user is allowed to do.

Authentication should always occur before authorization.

Business logic should rely on authenticated identity rather than performing authentication itself.

---

# Responsibilities

Authentication is responsible for:

- Verifying user identity.
- Establishing trust.
- Providing authenticated context.
- Protecting secured endpoints.
- Supporting secure communication.

Authentication is not responsible for enforcing business rules.

---

# Identity

Every authenticated request should carry a verified identity.

The identity should be available throughout the request lifecycle.

Business components should consume identity information without needing to understand how authentication is implemented.

Identity should be treated as part of the request context.

---

# Stateless Authentication

Authentication should support stateless communication whenever possible.

Each request should provide sufficient information for the server to verify the caller's identity.

The server should not depend on previous requests or session state to authenticate incoming requests.

Stateless authentication improves scalability and simplifies distributed architectures.

---

# Trust Boundary

Authentication should occur at the system boundary.

Once a request has been successfully authenticated, the remainder of the application should be able to trust the authenticated identity.

Internal components should not repeatedly authenticate the same request.

---

# Secure by Default

Protected resources should require authentication unless explicitly designed for anonymous access.

Public endpoints should remain the exception rather than the default.

Authentication should be applied consistently across the application.

---

# Credential Management

User credentials should be handled securely throughout their lifecycle.

The application should never expose or store credentials in a recoverable form.

Credential verification should always follow secure industry practices.

---

# Credential Recovery

Users should be able to recover access to their accounts without administrator intervention.

Recovery mechanisms should verify the user's identity through trusted communication channels before allowing credential changes.

Recovery processes should be temporary, secure, and resistant to misuse.

---

# Session Independence

Authentication should remain independent from business operations.

Business services should receive an authenticated identity without depending on authentication mechanisms or protocols.

Changes to the authentication implementation should not require modifications to business logic.

---

# Design Principles

Authentication follows these principles:

- Identity before permissions.
- Secure by default.
- Stateless communication.
- Separation of concerns.
- Least privilege.
- Trust at system boundaries.
- Implementation independence.

Authentication should protect the system without increasing unnecessary complexity.

---

# Design Goals

The authentication strategy aims to provide:

- Verified identities.
- Secure communication.
- Consistent access control.
- Scalable architecture.
- Maintainable security.
- Separation between infrastructure and business logic.

Authentication should enable secure interactions while remaining transparent to the application's business processes.

---

# Relationship with Other Documents

This document defines the authentication strategy of the API.

It should be read together with:

- APIOverview.md
- RESTConventions.md
- RequestResponse.md
- ErrorResponses.md
- 02-architecture/SecurityArchitecture.md

Together, these documents define how the system establishes identity and protects its public interfaces.

## OpenIddict configuration for this repository

The API uses ASP.NET Core Identity and OpenIddict with Authorization Code + PKCE. School-scoped records and users are isolated by the authenticated `school_id` claim. The API does not support public student self-registration; an administrator provisions an account and links it to an existing student or teacher record. `studentId` and `teacherId` are account links, not proof of identity.

### Local development

Development defaults to EF Core InMemory storage and development-only ephemeral OpenIddict keys. The development school is configured with `Development__SchoolId` and `Development__SchoolName`; no account or password is created by default. To bootstrap a local administrator, set `Authentication__BootstrapAdmin__Email`, `Authentication__BootstrapAdmin__Password`, and `Authentication__BootstrapAdmin__SchoolId` for the API process. The configured school must already exist. Never reuse development credentials in a deployed environment.

### Deployed environments

Outside Development, the API fails startup unless all required durable settings are present:

- `Database__Provider=SqlServer`
- `ConnectionStrings__SchoolDatabase` with the SQL Server connection string
- `Authentication__Issuer`, `Authentication__FrontendOrigin`, `Authentication__ClientId`, and `Authentication__AllowedOrigins__0` (and additional allowed origins as needed), all using HTTPS
- `Authentication__Certificates__SigningPath` and `Authentication__Certificates__SigningPassword`
- `Authentication__Certificates__EncryptionPath` and `Authentication__Certificates__EncryptionPassword`
- `Authentication__DataProtection__KeyRingPath`, pointing to an existing persistent directory shared by API instances

Provision private certificates and their passwords through the deployment's secret and certificate management system. Keep the Data Protection key ring on durable shared storage, protected by the configured encryption certificate. Do not commit certificate files, private keys, passwords, connection strings, or credentials.

The optional bootstrap administrator uses `Authentication__BootstrapAdmin__Email`, `Authentication__BootstrapAdmin__Password`, and `Authentication__BootstrapAdmin__SchoolId`. Its school must be initialized first. Do not configure this bootstrap mechanism as a public endpoint or put its values in source control.

### Database initialization

The repository contains versioned EF Core migrations. Apply them deliberately to the target SQL Server using the configured connection string and `dotnet ef database update`; application startup does not create or migrate a deployed database automatically. The initial schema does not assign fictional schools to existing records. Before importing any legacy records, create each real `Schools` row and explicitly associate every user and school-scoped record with its actual school. Configure the bootstrap administrator with the matching school ID. Existing development InMemory data is transient and is not migrated automatically.

For the React client, configure `VITE_OIDC_AUTHORITY`, `VITE_OIDC_CLIENT_ID`, and `VITE_API_URL` as described in `frontend/.env.example`.