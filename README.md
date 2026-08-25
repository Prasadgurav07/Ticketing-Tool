# Ticketing Tool

ASP.NET Core MVC ticket management system for internal IT/support workflows.

## Stack

- ASP.NET Core MVC with Razor views
- ASP.NET Core Identity with roles
- Entity Framework Core
- SQL Server through Entity Framework Core
- Bootstrap 5
- Local secure file storage under `SecureUploads/`

## Main Structure

- `Controllers/` - MVC controllers for dashboards, tickets, support, admin, notifications
- `Models/` - Identity user plus ticketing domain entities
- `ViewModels/` - screen and form models
- `Services/` - business logic for tickets, status transitions, notifications, file storage, ticket numbers
- `Data/` - EF Core `ApplicationDbContext` and seed data
- `Migrations/` - EF Core database migration
- `Views/` - Razor UI
- `wwwroot/` - Bootstrap assets, CSS, JS

## Setup

1. Update `ConnectionStrings:DefaultConnection` in `appsettings.json` for your SQL Server. The current file points to `DESKTOP-NGALKNG\SQLEXPRESS`.
2. Run the migration:

   ```powershell
   dotnet ef database update
   ```

3. Start the app:

   ```powershell
   dotnet run
   ```

The app also calls `Database.MigrateAsync()` on startup, so pending migrations are applied automatically when the application starts.

LocalDB example:

```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Ticketing_Tool;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False;TrustServerCertificate=True"
```

## Seed Data

The seed routine creates:

- Roles: `Employee`, `Support Agent`, `Team Lead`, `Admin`
- Departments: IT, HR, Finance, Operations, Sales, Admin
- Teams: Network Support, IT Helpdesk, Application Support, Operations Support
- Categories: Hardware, Software, Network, Email, Access, Application, Database, Other
- Priorities: Critical, High, Medium, Low
- Statuses: Open, Assigned, In Progress, Pending User, Resolved, Closed, Cancelled

Development admin account from `appsettings.Development.json`:

- Email: `admin@company.local`
- Password: `ChangeThisDevPassword!2026`

Replace this using user secrets or environment variables before sharing beyond local development.

## Notes

- Attachments are stored outside `wwwroot` and downloaded through authorized controller actions.
- Ticket numbers use the format `INC-YYYY-000001`.
- Status transition rules are centralized in `Services/StatusTransitionService.cs`.
- Admin and support actions are protected server-side with role authorization.
