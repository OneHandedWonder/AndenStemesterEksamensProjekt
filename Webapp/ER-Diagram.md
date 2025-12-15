# ER-Diagram for Zealand Eksamens Oversigt Database

## Entity Relationship Diagram

```mermaid
---
id: b10b27fc-23f7-47e2-a4fc-f8f138ba29c5
---
erDiagram
    USERS ||--o| PROFILES : "has"
    USERS ||--o{ SESSIONS : "has"
    USERS ||--o{ CALENDAR_EVENTS : "creates"
    USERS ||--o{ EVENT_PARTICIPANTS : "participates"
    USERS }o--o{ TEAMS : "belongs to"
    CALENDAR_EVENTS ||--o{ EVENT_PARTICIPANTS : "includes"
    USER_TEAMS }o--|| USERS : "references"
    USER_TEAMS }o--|| TEAMS : "references"

    USERS {
        int uid PK
        varchar email UK
        varchar firstName
        varchar lastName
        varchar password_hash
        timestamp created_at
        timestamp updated_at
        timestamp last_login
        boolean is_active
        varchar role
    }

    PROFILES {
        int puid PK
        int uid "FK, UK"
        varchar navn
        varchar adresse
        varchar mobil_nr
        timestamp created_at
        timestamp updated_at
    }

    SESSIONS {
        int session_id PK
        int uid FK
        char session_token
        timestamp created_at
        timestamp expires_at
    }

    CALENDAR_EVENTS {
        int event_id PK
        int user_id FK
        varchar title
        varchar description
        timestamp submission_time
        timestamp start_time
        timestamp end_time
        varchar location
        boolean is_all_day
        varchar type
        varchar color
        timestamp created_at
        timestamp updated_at
    }

    EVENT_PARTICIPANTS {
        int participant_id PK
        int event_id FK
        int user_id FK
        varchar status
        timestamp joined_at
    }

    TEAMS {
        int id PK
        varchar name
        text description
        timestamp created_at
        timestamp updated_at
    }

    USER_TEAMS {
        int user_id "PK, FK"
        int team_id "PK, FK"
        timestamp joined_at
    }
```

## Relationships Explained

### One-to-One Relationships
- **USERS → PROFILES**: Each user has one profile (1:1)
  - FK: `profiles.uid` → `users.uid`

### One-to-Many Relationships
- **USERS → SESSIONS**: One user can have multiple active sessions (1:N)
  - FK: `sessions.uid` → `users.uid`

- **USERS → CALENDAR_EVENTS**: One user can create multiple calendar events (1:N)
  - FK: `calendar_events.user_id` → `users.uid`

- **CALENDAR_EVENTS → EVENT_PARTICIPANTS**: One event can have multiple participants (1:N)
  - FK: `event_participants.event_id` → `calendar_events.event_id`

### Many-to-Many Relationships
- **USERS ↔ EVENT_PARTICIPANTS**: Users can participate in multiple events (M:N)
  - Junction: `event_participants` table
  - FKs: `event_participants.user_id` → `users.uid`
  - FKs: `event_participants.event_id` → `calendar_events.event_id`

- **USERS ↔ TEAMS**: Users can belong to multiple teams, teams can have multiple users (M:N)
  - Junction: `user_teams` table
  - Composite PK: (`user_id`, `team_id`)
  - FKs: `user_teams.user_id` → `users.uid`
  - FKs: `user_teams.team_id` → `teams.id`

## Database Constraints

### Check Constraints
- **users.role**: Must be one of: `guest`, `student`, `lecturer`, `planner`, `censor`, `admin`
- **calendar_events.type**: Must be one of: `written`, `oral`, `oral+written`, `project`
- **event_participants.status**: Must be one of: `pending`, `accepted`, `declined`

### Unique Constraints
- **users.email**: Each email must be unique
- **profiles.uid**: Each user can only have one profile
- **event_participants (event_id, user_id)**: A user can only participate once per event
- **user_teams (user_id, team_id)**: Composite primary key ensures unique team membership

### Cascade Rules
All foreign keys use `ON DELETE CASCADE`, meaning:
- Deleting a user automatically deletes their sessions, profile, events, and team memberships
- Deleting an event automatically deletes all participant records
- Deleting a team automatically removes all user memberships

## Indexes

### users
- `idx_users_email` on `email`
- `idx_users_active` on `is_active`

### sessions
- `idx_session_token` on `session_token`
- `idx_session_uid` on `uid`

### calendar_events
- `idx_calendar_events_user_id` on `user_id`
- `idx_calendar_events_start_time` on `start_time`

### event_participants
- `idx_event_participants_event_id` on `event_id`
- `idx_event_participants_user_id` on `user_id`
- `idx_event_participants_status` on `status`

### user_teams
- `idx_user_teams_user_id` on `user_id`
- `idx_user_teams_team_id` on `team_id`
