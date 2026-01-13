# Database Schema Documentation

## Entity Relationship Diagram

```mermaid
erDiagram
    Tenants ||--o{ Users : "has many"
    Tenants ||--o{ Invitations : "has many"
    Tenants ||--o{ AuditLogs : "has many"
    Tenants ||--o{ OnboardingSurveys : "has many"

    Users ||--o{ RefreshTokens : "has many"
    Users ||--o{ Invitations : "invited"
    Users ||--o{ AuditLogs : "performed"
    Users ||--o{ OnboardingResponses : "submitted"
    Users ||--o| Users : "invited by"

    OnboardingSurveys ||--o{ OnboardingSurveyVersions : "has versions"
    OnboardingSurveys ||--o| OnboardingSurveyVersions : "active version"

    OnboardingSurveyVersions ||--o{ OnboardingResponses : "has responses"

    Tenants {
        uuid Id PK
        string Slug UK
        string Name
        string SubscriptionTier
        text Settings
        text Branding
        timestamp CreatedAt
        boolean IsActive
    }

    Users {
        uuid Id PK
        uuid TenantId FK
        string Email UK
        text PasswordHash
        string Name
        string Role
        uuid InvitedBy FK
        timestamp InvitedAt
        timestamp LastLoginAt
        timestamp CreatedAt
        boolean IsActive
    }

    Invitations {
        uuid Id PK
        uuid TenantId FK
        string Email
        string Role
        string Token UK
        timestamp ExpiresAt
        timestamp AcceptedAt
        uuid InvitedBy FK
        timestamp CreatedAt
    }

    RefreshTokens {
        uuid Id PK
        uuid UserId FK
        string Token UK
        timestamp ExpiresAt
        timestamp CreatedAt
        timestamp RevokedAt
    }

    AuditLogs {
        uuid Id PK
        uuid TenantId FK
        uuid UserId FK
        string Action
        string EntityType
        uuid EntityId
        text Metadata
        timestamp CreatedAt
    }

    OnboardingSurveys {
        uuid Id PK
        uuid TenantId FK
        string Name
        int CurrentVersionNumber
        uuid ActiveVersionId FK
        boolean IsActive
        timestamp CreatedAt
        timestamp UpdatedAt
    }

    OnboardingSurveyVersions {
        uuid Id PK
        uuid SurveyId FK
        int VersionNumber
        text SurveyJson
        timestamp CreatedAt
    }

    OnboardingResponses {
        uuid Id PK
        uuid SurveyVersionId FK
        uuid UserId FK
        text ResponseJson
        boolean IsComplete
        timestamp StartedAt
        timestamp CompletedAt
    }
```

## Tables

### Tenants
The root entity for multi-tenancy. All other entities belong to a tenant.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| Slug | varchar | NO | Unique URL-friendly identifier (e.g., "acme") |
| Name | varchar | NO | Display name |
| SubscriptionTier | varchar | NO | Subscription level (starter, professional, enterprise) |
| Settings | text | YES | JSON settings (notifications, timezone, language) |
| Branding | text | YES | JSON branding config (colors, logo) |
| CreatedAt | timestamp | NO | Creation timestamp |
| IsActive | boolean | NO | Whether tenant is active |

### Users
User accounts belonging to a tenant.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| TenantId | uuid | NO | FK to Tenants |
| Email | varchar | NO | Unique within tenant |
| PasswordHash | text | NO | BCrypt hashed password |
| Name | varchar | NO | Display name |
| Role | varchar | NO | User role (Viewer, Member, Admin) |
| InvitedBy | uuid | YES | FK to Users (self-referential) |
| InvitedAt | timestamp | YES | When user was invited |
| LastLoginAt | timestamp | YES | Last login timestamp |
| CreatedAt | timestamp | NO | Creation timestamp |
| IsActive | boolean | NO | Whether user is active |

### Invitations
Pending user invitations.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| TenantId | uuid | NO | FK to Tenants |
| Email | varchar | NO | Invited email address |
| Role | varchar | NO | Role to assign on acceptance |
| Token | varchar | NO | Unique invitation token |
| ExpiresAt | timestamp | NO | Expiration timestamp |
| AcceptedAt | timestamp | YES | When invitation was accepted |
| InvitedBy | uuid | NO | FK to Users |
| CreatedAt | timestamp | NO | Creation timestamp |

### RefreshTokens
JWT refresh tokens for authentication.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| UserId | uuid | NO | FK to Users |
| Token | varchar | NO | Unique refresh token |
| ExpiresAt | timestamp | NO | Expiration timestamp |
| CreatedAt | timestamp | NO | Creation timestamp |
| RevokedAt | timestamp | YES | When token was revoked |

### AuditLogs
Activity audit trail.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| TenantId | uuid | NO | FK to Tenants |
| UserId | uuid | YES | FK to Users (nullable for system actions) |
| Action | varchar | NO | Action performed |
| EntityType | varchar | NO | Type of entity affected |
| EntityId | uuid | YES | ID of entity affected |
| Metadata | text | YES | JSON additional data |
| CreatedAt | timestamp | NO | When action occurred |

### OnboardingSurveys
Survey/form definitions per tenant. Each tenant has one survey with multiple versions.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| TenantId | uuid | NO | FK to Tenants |
| Name | varchar | NO | Survey display name |
| CurrentVersionNumber | int | NO | Latest version number |
| ActiveVersionId | uuid | YES | FK to OnboardingSurveyVersions (currently active) |
| IsActive | boolean | NO | Whether survey is enabled |
| CreatedAt | timestamp | NO | Creation timestamp |
| UpdatedAt | timestamp | NO | Last update timestamp |

### OnboardingSurveyVersions
Versioned survey configurations. Preserves history when surveys are modified.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| SurveyId | uuid | NO | FK to OnboardingSurveys |
| VersionNumber | int | NO | Version number (1, 2, 3...) |
| SurveyJson | text | NO | SurveyJS JSON configuration |
| CreatedAt | timestamp | NO | When version was created |

**Unique constraint:** (SurveyId, VersionNumber)

### OnboardingResponses
New starter form submissions. Links to specific survey version.

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | uuid | NO | Primary key |
| SurveyVersionId | uuid | NO | FK to OnboardingSurveyVersions |
| UserId | uuid | NO | FK to Users (who submitted) |
| ResponseJson | text | NO | JSON form responses |
| IsComplete | boolean | NO | Whether submission is complete |
| StartedAt | timestamp | NO | When submission started |
| CompletedAt | timestamp | YES | When submission completed |

## Foreign Key Relationships

| From Table | Column | To Table | Column | Description |
|------------|--------|----------|--------|-------------|
| Users | TenantId | Tenants | Id | User belongs to tenant |
| Users | InvitedBy | Users | Id | Self-referential: who invited this user |
| Invitations | TenantId | Tenants | Id | Invitation belongs to tenant |
| Invitations | InvitedBy | Users | Id | Who created the invitation |
| RefreshTokens | UserId | Users | Id | Token belongs to user |
| AuditLogs | TenantId | Tenants | Id | Log belongs to tenant |
| AuditLogs | UserId | Users | Id | Who performed the action |
| OnboardingSurveys | TenantId | Tenants | Id | Survey belongs to tenant |
| OnboardingSurveys | ActiveVersionId | OnboardingSurveyVersions | Id | Currently active version |
| OnboardingSurveyVersions | SurveyId | OnboardingSurveys | Id | Version belongs to survey |
| OnboardingResponses | SurveyVersionId | OnboardingSurveyVersions | Id | Response submitted against specific version |
| OnboardingResponses | UserId | Users | Id | Who submitted the response |

## Multi-Tenancy

All data is isolated by tenant using:
1. **TenantId foreign key** on most tables
2. **Global query filters** in EF Core that automatically filter by current tenant

Query filters are defined in `AppDbContext.cs`:
```csharp
modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId == _tenantContext.TenantId);
modelBuilder.Entity<OnboardingSurvey>().HasQueryFilter(s => s.TenantId == _tenantContext.TenantId);
modelBuilder.Entity<OnboardingResponse>().HasQueryFilter(r => r.SurveyVersion.Survey.TenantId == _tenantContext.TenantId);
```

## Survey Versioning Flow

```mermaid
sequenceDiagram
    participant Admin
    participant API
    participant DB

    Note over Admin,DB: Create Survey
    Admin->>API: POST /api/onboarding/admin/survey
    API->>DB: INSERT OnboardingSurveys (version=1)
    API->>DB: INSERT OnboardingSurveyVersions (v1)
    API->>DB: UPDATE OnboardingSurveys SET ActiveVersionId

    Note over Admin,DB: User Submits Response
    Admin->>API: POST /api/onboarding/response
    API->>DB: Get ActiveVersionId from survey
    API->>DB: INSERT OnboardingResponses (SurveyVersionId=v1)

    Note over Admin,DB: Update Survey (creates new version)
    Admin->>API: PUT /api/onboarding/admin/survey
    API->>DB: INSERT OnboardingSurveyVersions (v2)
    API->>DB: UPDATE OnboardingSurveys SET CurrentVersionNumber=2, ActiveVersionId=v2

    Note over Admin,DB: New Response Uses New Version
    Admin->>API: POST /api/onboarding/response
    API->>DB: INSERT OnboardingResponses (SurveyVersionId=v2)
```
