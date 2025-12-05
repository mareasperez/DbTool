This repository contains DbTool — a multi-engine DB backup & restore app written in .NET 9.
Keep instructions short and focused on patterns an AI coder needs to be productive.

1) Big-picture architecture
- **Layers:** `src/DbTool.CLI` → `DbTool.Application` → `DbTool.Infrastructure` → `DbTool.Domain`.
- **Purpose:** CLI implements commands and wiring (`Program.cs`); Application contains DTOs, service interfaces and validators; Infrastructure contains implementations (providers, repositories, services); Domain contains entities and interfaces used across layers.

2) Key files & symbols to reference
- `src/DbTool.CLI/Program.cs` — command definitions (System.CommandLine), Serilog setup, `ServiceCollection` wiring and `services.Configure<DbToolSettings>`.
- `src/DbTool.Infrastructure/DependencyInjection.cs` — central DI registrations (use `AddInfrastructure()` to load repos/services). Note: `AddInfrastructure(string? dbPath = null)` accepts an override DB path useful for tests.
- `src/DbTool.Infrastructure/Data/AppDbContext.cs` — SQLite config DB location logic (prefers `Environment.ProcessPath` for single-file builds), PRAGMA settings, and schema initialization.
- `src/DbTool.Infrastructure/Providers/*` — implement `IDatabaseProvider` (e.g. `MySqlProvider.cs`) with `BackupAsync`, `RestoreAsync`, `DropAllTablesAsync`, `TestConnectionAsync`.
- `src/DbTool.Application/DTOs`, `Interfaces`, `Validators` — DTO shapes and FluentValidation usage (validators are scanned via `AddValidatorsFromAssemblyContaining<T>`).

3) Important conventions & patterns
- Providers implement `IDatabaseProvider` and must produce plain SQL backups (human-readable). See `MySqlProvider` for how to produce CREATE + INSERT statements.
- Repositories use `AppDbContext` + Dapper (`src/DbTool.Infrastructure/Repositories`).
- Compression is exposed via `ICompressionService` and the default is `GzipCompressionService` (registered as singleton).
- Use Options pattern for configuration: `DbToolSettings` is bound in `Program.cs` to the `DbTool` section in `appsettings.json`.
- Tests often override the SQLite path by calling `AddInfrastructure(dbPath)` or injecting a custom `AppDbContext`.

4) Build / run / test commands (Windows PowerShell examples used by project)
- Build: `dotnet build` (solution root) or `dotnet build src/DbTool.CLI`
- Run CLI (examples in `README.md`):
  - Add connection: `dotnet run --project src/DbTool.CLI -- db add --name prod --engine postgres --host localhost --port 5432 --database myapp --username u --password p`
  - Backup: `dotnet run --project src/DbTool.CLI -- backup --db prod`
- Run tests: `dotnet test` (root runs all test projects)
- Create self-contained binaries: `dotnet publish src/DbTool.CLI -c Release -r <rid> --self-contained -p:PublishSingleFile=true -o ./dist/<rid>`

5) Runtime behavior & gotchas (important to preserve)
- Config DB path: `AppDbContext.GetDefaultDbPath()` prefers `Environment.ProcessPath` (single-file exe) then `AppContext.BaseDirectory`; keep this logic when changing AppDbContext or when writing tests that inspect on-disk paths.
- SQLite PRAGMA settings are set to `journal_mode=DELETE` and `synchronous=FULL` — these are deliberate choices for durability.
- Backup files are plain SQL. Avoid adding binary formats unless a migration plan exists.
- Restore CLI prompts for confirmation unless `--force` is used (`Program.cs` restore command). Keep that safety check when modifying restore flows.

6) Adding a new database provider
 - Implement `IDatabaseProvider` in `src/DbTool.Infrastructure/Providers`.
 - Add provider registration where `ProviderFactory` (or similar) is used (search `EngineName`/`DatabaseEngineType`).
 - Add driver NuGet to `DbTool.Infrastructure.csproj` and update `DatabaseEngineType` enum in `DbTool.Domain/Enums`.
 - Follow `MySqlProvider` for examples: produce CREATE statements, generate INSERTs, disable/enable FK checks where appropriate.

7) Where to change behavior
- DI registrations: `DependencyInjection.AddInfrastructure`.
- CLI surface: `Program.cs` (command structure and user-facing messages).
- Persistence schema & default path: `AppDbContext.cs`.
- Configuration model: `src/DbTool.Application/Settings/DbToolSettings.cs` and `appsettings.json`.

8) Useful search terms for AI agents
- `IDatabaseProvider`, `IBackupService`, `IDatabaseConnectionService`, `AppDbContext`, `DependencyInjection`, `CreateDatabaseConnectionDto`, `GzipCompressionService`, `DatabaseEngineType`.

If anything here is unclear or you want more examples (e.g., sample unit test that overrides `dbPath`), tell me which area to expand and I'll iterate.
