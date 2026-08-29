# Copilot Instructions for GameStore API

## Project Overview

GameStore API is a minimal ASP.NET Core 10.0 REST API for managing a game catalog. It demonstrates clean architecture principles with Entity Framework Core (EF Core) for data access and SQLite as the database.

## Build & Test Commands

**Build the project:**
```bash
dotnet build
```

**Run the API:**
```bash
dotnet run --project GameStore.Api
```
The API starts on `https://localhost:7001` (or `http://localhost:5000` for HTTP).

**Database Operations:**
```bash
# Apply migrations (runs automatically on startup, but can be run manually)
dotnet ef database update --project GameStore.Api

# Create a new migration after model changes
dotnet ef migrations add <MigrationName> --project GameStore.Api

# Remove the last migration (if not applied)
dotnet ef migrations remove --project GameStore.Api
```

## Architecture

### Minimal APIs Pattern
The project uses ASP.NET Core's minimal APIs (not traditional Controllers). All endpoints are defined in extension methods:
- **Endpoints** are grouped in static classes with MapXxxEndpoints() extension methods
- **Routes** are registered in `Program.cs` by calling these extension methods
- **Route grouping** uses `app.MapGroup()` to organize related endpoints under a common prefix

### Layered Structure
```
GameStore.Api/
├── Program.cs                 # Configuration, DI setup, route registration
├── Models/                    # EF Core entity models (Game, Genre)
├── Dtos/                      # Data Transfer Objects for API contracts
├── Endpoints/                 # API endpoint definitions
├── Data/                      # EF Core context and database extensions
└── Properties/                # Assembly info
```

### Data Flow
1. **Models** (Models/) represent database entities with EF Core mapping
2. **DTOs** (Dtos/) define request/response contracts using C# Records
3. **Endpoints** (Endpoints/) handle HTTP requests, validate DTOs, and call business logic
4. **DbContext** (Data/GameStoreContext.cs) manages EF Core entity mapping
5. **Migrations** run automatically on startup via DataExtensions.MigrateDB()

## Key Conventions

### DTOs and Records
- All DTOs are **C# Records** (immutable, with value-based equality)
- Input DTOs (CreateGameDto, UpdateGameDto) use `[Required]` and `[Range]`/`[StringLength]` validation attributes
- DTOs are the contract between client and server; Models are internal representation

**Example pattern:**
```csharp
public record CreateGameDto(
    [Required][StringLength(50)] string Name,
    [Required][StringLength(20)] string Genre,
    [Range(1, 100)] decimal Price,
    DateOnly ReleaseDate
);
```

### Entity Models
- Required properties use the `required` keyword (not nullable or optional)
- Nullable reference types are **enabled** project-wide (`<Nullable>enable</Nullable>`)
- Foreign key relationships use `GenreId` naming convention (EF Core auto-discovers these)
- Use `DateOnly` for date-only values, not `DateTime`

**Example pattern:**
```csharp
public class Game
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public Genre? Genre { get; set; }            // Navigation property
    public int GenreId { get; set; }             // Foreign key
    public decimal Price { get; set; }
    public DateOnly ReleaseDate { get; set; }
}
```

### Endpoints Pattern
- All endpoint methods live in **static classes** (not instances)
- Use extension methods on `WebApplication` (e.g., `MapGamesEndpoints(this WebApplication app)`)
- Use `MapGroup()` to group related endpoints under a common prefix
- Return `Results.*` helper methods for all responses (NotFound, Ok, CreatedAtRoute, NoContent)

**Example pattern:**
```csharp
public static void MapGamesEndpoints(this WebApplication app)
{
    var group = app.MapGroup("/games");
    group.MapGet("/", () => games);
    group.MapGet("/{id}", (int id) => game is null ? Results.NotFound() : Results.Ok(game))
        .WithName("GetGame");
    group.MapPost("/", (CreateGameDto dto) => /* ... */);
}
```

### Dependency Injection
- DI is configured in `Program.cs` via `builder.Services.Add*()`
- Database connection string is defined in `Program.cs`
- Use constructor injection in classes (primary constructor syntax is preferred with C# 10+)

**Example pattern:**
```csharp
public class GameStoreContext(DbContextOptions<GameStoreContext> options) 
    : DbContext(options) { }
```

### Database Migrations
- Migrations are applied **automatically on startup** via `app.MigrateDB()` in Program.cs
- Don't manually manage migration application in normal workflows
- SQLite database file (`GameStore.db`) is gitignored

## Code Style Notes

- **Implicit usings** are enabled; no need for `System;`, `System.Collections;`, etc.
- **Decimal literal suffix**: Use `m` for decimal values (e.g., `19.99m`)
- **Records** are preferred for DTOs and immutable value types
- **Comments explain "why"**, not obvious code mechanics
- Keep endpoint methods concise; extract complex logic to service classes if needed

## Known Considerations

- Thread-safety for PUT requests is noted but not yet implemented—consider adding in future updates
- The `AddValidation()` extension in Program.cs requires a custom implementation or NuGet package setup
