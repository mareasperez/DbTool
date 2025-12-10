# DbTool

> **Multi-Engine Database Backup & Restore Tool**  
> Cross-platform • Zero Dependencies • Native .NET Drivers • CLI & GUI

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![GitHub Release](https://img.shields.io/github/v/release/mareasperez/DbTool)](https://github.com/mareasperez/DbTool/releases)
[![GitHub Actions](https://img.shields.io/github/actions/workflow/status/mareasperez/DbTool/release.yml)](https://github.com/mareasperez/DbTool/actions)

---

## 🚀 Quick Start

### Using the GUI (Desktop Application)

```powershell
# Build and run the UI
dotnet run --project src/DbTool.UI
```

The GUI provides a user-friendly interface with three main sections:
- **Connections**: Manage database connections (add, test, delete)
- **Backup**: Create and view backups for your databases
- **Restore**: Restore databases from backup files

### Using the CLI

```powershell
# Build the project
dotnet build

# Add a database connection
dotnet run --project src/DbTool.CLI -- db add \
  --name prod \
  --engine postgres \
  --host localhost \
  --port 5432 \
  --database myapp \
  --username postgres \
  --password yourpass

# Create a backup
dotnet run --project src/DbTool.CLI -- backup --db prod

# List all backups
dotnet run --project src/DbTool.CLI -- list-backups --db prod
```

---

## ✨ Features

- ✅ **Zero External Dependencies** - No pg_dump, mysqldump, or sqlcmd required
- ✅ **Multi-Database Support** - PostgreSQL, MySQL, SQL Server, MariaDB
- ✅ **Cross-Platform** - Works on Windows, Linux, macOS
- ✅ **Native .NET Drivers** - Uses official database drivers (Npgsql, MySqlConnector, etc.)
- ✅ **Clean Architecture** - N-tier design with Domain, Application, Infrastructure, CLI, UI layers
- ✅ **Self-Contained** - Can be published as a single executable
- ✅ **SQLite Configuration** - Local database for connection management
- ✅ **Optional Compression** - Gzip compression (disabled by default)
- ✅ **CLI & GUI** - Command-line interface and Avalonia desktop application
- ✅ **Automated Releases** - GitHub Actions workflow for multi-platform builds

---

## 📦 Supported Databases

| Database | Driver | Status |
|----------|--------|--------|
| PostgreSQL | Npgsql 9.0.2 | ✅ |
| MySQL | MySqlConnector 2.4.0 | ✅ |
| SQL Server | Microsoft.Data.SqlClient 5.2.2 | ✅ |
| MariaDB | MySqlConnector 2.4.0 | ✅ |

---

## 📖 Usage

### GUI Application

The DbTool GUI provides an intuitive interface for managing database operations:

#### Connections Tab
- **Add Connection**: Fill in the form with connection details (name, engine, host, port, database, username, password)
- **View Connections**: See all configured database connections in a list
- **Test Connection**: Verify connectivity to a selected database
- **Delete Connection**: Remove a database connection

#### Backup Tab
- **Create Backup**: Select a connection and optionally specify an output directory
- **View Backup History**: Browse all backups for the selected connection with timestamps and file sizes
- **Refresh List**: Update the backup history view

#### Restore Tab
- **Restore Database**: Select a connection and backup file to restore
- **Safety Warning**: Clear warnings about data overwrite operations

### CLI Commands

#### Database Connection Management

```powershell
# Add a new connection
dotnet run --project src/DbTool.CLI -- db add \
  --name <connection-name> \
  --engine <postgres|mysql|sqlserver|mariadb> \
  --host <hostname> \
  --port <port> \
  --database <database-name> \
  --username <username> \
  --password <password>

# List all connections
dotnet run --project src/DbTool.CLI -- db list

# Test a connection
dotnet run --project src/DbTool.CLI -- db test --name <connection-name>

# Delete a connection
dotnet run --project src/DbTool.CLI -- db delete --name <connection-name>

# View configuration information
dotnet run --project src/DbTool.CLI -- db info
```

#### Backup Operations

```powershell
# Create a backup
dotnet run --project src/DbTool.CLI -- backup --db <connection-name>

# Create a backup with custom output directory
dotnet run --project src/DbTool.CLI -- backup --db <connection-name> --output /path/to/backups

# List all backups for a database
dotnet run --project src/DbTool.CLI -- list-backups --db <connection-name>
```

#### Restore Operations

```powershell
# Restore from a backup file (with confirmation prompt)
dotnet run --project src/DbTool.CLI -- restore --db <connection-name> --file <backup-file-path>

# Restore without confirmation (use with caution!)
dotnet run --project src/DbTool.CLI -- restore --db <connection-name> --file <backup-file-path> --force
```

> **⚠️ Warning**: Restore operations will overwrite existing data. Always verify the backup file before restoring.

---

## 🏗️ Architecture

```
DbTool/
├── src/
│   ├── DbTool.Domain/          # Entities, Enums, Interfaces (pure C#)
│   ├── DbTool.Application/     # DTOs, Service Interfaces, Validators
│   ├── DbTool.Infrastructure/  # Repositories, Providers, Services
│   ├── DbTool.CLI/             # Command-line interface
│   └── DbTool.UI/              # Avalonia GUI application
└── tests/
    ├── DbTool.Domain.Tests/
    ├── DbTool.Application.Tests/
    └── DbTool.Infrastructure.Tests/
```

**Dependency Flow**: `CLI/UI → Application → Infrastructure → Domain`

The UI application uses:
- **Avalonia 11.3.9** - Cross-platform XAML-based UI framework
- **CommunityToolkit.Mvvm 8.2.1** - MVVM helpers and commands
- **Microsoft.Extensions.DependencyInjection** - Built-in DI container

---

## 🔧 Installation

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) or later

### Build from Source

```powershell
# Clone the repository
git clone https://github.com/mareasperez/DbTool.git
cd DbTool

# Restore dependencies and build
dotnet restore
dotnet build

# Run the CLI
dotnet run --project src/DbTool.CLI -- --help

# Run the UI
dotnet run --project src/DbTool.UI
```

### Create Self-Contained Executables

#### CLI

```powershell
# Windows
dotnet publish src/DbTool.CLI -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./dist/cli-win

# Linux
dotnet publish src/DbTool.CLI -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o ./dist/cli-linux

# macOS
dotnet publish src/DbTool.CLI -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true -o ./dist/cli-mac
```

#### GUI

```powershell
# Windows
dotnet publish src/DbTool.UI -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./dist/ui-win

# Linux
dotnet publish src/DbTool.UI -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o ./dist/ui-linux

# macOS
dotnet publish src/DbTool.UI -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true -o ./dist/ui-mac
```

The executables will be in the `dist` folder (CLI ~70MB, UI ~80MB, includes all dependencies).

---

## 📁 Configuration

Database connections are stored in a local SQLite database:

- **Windows**: `%APPDATA%\DbTool\config.db`
- **Linux/macOS**: `~/.config/DbTool/config.db`

---

## 🧪 Testing

```powershell
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📊 Backup Format

Backups are generated as **plain SQL files** containing:
- CREATE TABLE statements (full schema definition)
- INSERT statements (all data)
- Database-specific optimizations

**Advantages**:
- Human-readable and easy to inspect
- Version control friendly
- Can be modified before restore
- Cross-platform compatible

---

## 🛠️ Development

### Project Structure

- **Domain**: Core business entities and interfaces (no external dependencies)
- **Application**: Use cases, DTOs, and service contracts
- **Infrastructure**: Database implementations, providers, and services
- **CLI**: Command-line interface using System.CommandLine
- **UI**: Desktop application using Avalonia UI framework

### Adding a New Database Provider

1. Create a new provider class implementing `IDatabaseProvider`
2. Add the provider to `ProviderFactory`
3. Add the corresponding NuGet package to `DbTool.Infrastructure.csproj`
4. Update `DatabaseEngineType` enum

Example:
```csharp
public class OracleProvider : IDatabaseProvider
{
    public string EngineName => "oracle";
    
    public async Task BackupAsync(DatabaseConnection connection, string outputPath, ...)
    {
        // Implementation using Oracle.ManagedDataAccess.Core
    }
    
    // Implement other methods...
}
```

---

## 📦 Releases

### Download Pre-built Binaries

Pre-compiled binaries are available for download on the [Releases page](https://github.com/mareasperez/DbTool/releases).

**Available platforms**:
- **CLI**: Windows (x64), Linux (x64), macOS (x64)
- **UI**: Windows (x64), Linux (x64), macOS (x64, ARM64)

Each release includes self-contained executables that don't require .NET to be installed.

### Creating a New Release

Releases are automatically created when you push a version tag:

```bash
# Create a new version tag
git tag v1.0.0

# Push the tag to GitHub
git push origin v1.0.0
```

This will trigger the GitHub Actions workflow that:
1. Builds the CLI and UI for all platforms
2. Creates self-contained single-file executables
3. Packages them as ZIP files
4. Creates a draft release with all binaries attached

After the workflow completes, you can edit the draft release notes and publish it.

---

## 🚧 Roadmap

### ✅ Completed
- [x] Implement restore functionality
- [x] Build GUI with Avalonia
- [x] Automated releases (GitHub Actions)
- [x] Multi-platform support (Windows, Linux, macOS)
- [x] Self-contained single-file executables
- [x] Full-featured UI with connection management, backup, and restore

### 🔄 In Progress
- [ ] Add backup compression support
- [ ] Add more database providers (Oracle, MongoDB)

### 📋 Planned
- [ ] Backup encryption
- [ ] Scheduled backups
- [ ] Cloud storage integration (Azure, AWS S3)
- [ ] Backup retention policies
- [ ] Email notifications
- [ ] Backup verification
- [ ] Incremental backups
- [ ] Backup diff/comparison tools
- [ ] File browser dialog in UI for backup selection
- [ ] Dark mode support in UI

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- [Npgsql](https://www.npgsql.org/) - PostgreSQL .NET driver
- [MySqlConnector](https://mysqlconnector.net/) - MySQL .NET driver
- [Microsoft.Data.SqlClient](https://github.com/dotnet/SqlClient) - SQL Server .NET driver
- [Dapper](https://github.com/DapperLib/Dapper) - Micro ORM
- [FluentValidation](https://fluentvalidation.net/) - Validation library
- [System.CommandLine](https://github.com/dotnet/command-line-api) - Command-line parsing
- [Avalonia](https://avaloniaui.net/) - Cross-platform XAML-based UI framework
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) - MVVM helpers

---

## 📞 Support

For issues, questions, or suggestions, please [open an issue](https://github.com/mareasperez/DbTool/issues).

---

**Made with ❤️ using .NET**
