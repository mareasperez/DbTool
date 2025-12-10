using System.CommandLine;
using System.Text;
using DbTool.Application.DTOs;
using DbTool.Application.Interfaces;
using DbTool.Application.Settings;
using DbTool.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

// Configure console encoding for Windows to support Unicode characters
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// Configure Serilog
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "dbtool-.log");
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}")
    .WriteTo.File(logPath, 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

Log.Information("DbTool starting...");


// Load configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();

// Configure Options Pattern
services.Configure<DbToolSettings>(configuration.GetSection("DbTool"));

// Register Serilog logger
services.AddSingleton(Log.Logger);

services.AddInfrastructure();
var serviceProvider = services.BuildServiceProvider();

var rootCommand = new RootCommand("DbTool - Multi-Engine Database Backup & Restore Tool");

// Database commands
var dbCommand = new Command("db", "Manage database connections");

// db add
var dbAddCommand = new Command("add", "Add a new database connection");
var nameOption = new Option<string>("--name", "Connection name") { IsRequired = true };
var engineOption = new Option<string>("--engine", "Database engine (postgres, mysql, sqlserver, mariadb)") { IsRequired = true };
var hostOption = new Option<string>("--host", "Database host") { IsRequired = true };
var portOption = new Option<int>("--port", "Database port") { IsRequired = true };
var databaseOption = new Option<string>("--database", "Database name") { IsRequired = true };
var usernameOption = new Option<string>("--username", "Database username") { IsRequired = true };
var passwordOption = new Option<string>("--password", "Database password") { IsRequired = true };

dbAddCommand.AddOption(nameOption);
dbAddCommand.AddOption(engineOption);
dbAddCommand.AddOption(hostOption);
dbAddCommand.AddOption(portOption);
dbAddCommand.AddOption(databaseOption);
dbAddCommand.AddOption(usernameOption);
dbAddCommand.AddOption(passwordOption);

dbAddCommand.SetHandler(async (name, engine, host, port, database, username, password) =>
{
    var dbService = serviceProvider.GetRequiredService<IDatabaseConnectionService>();
    var logger = serviceProvider.GetRequiredService<Serilog.ILogger>();
    
    try
    {
        var dto = new CreateDatabaseConnectionDto(name, engine, host, port, database, username, password);
        var id = await dbService.CreateDatabaseConnectionAsync(dto);
        logger.Information("✓ Database connection '{Name}' created successfully (ID: {Id})", name, id);
    }
    catch (Exception ex)
    {
        logger.Error(ex, "✗ Error: {Message}", ex.Message);
        Environment.Exit(1);
    }
}, nameOption, engineOption, hostOption, portOption, databaseOption, usernameOption, passwordOption);

// db list
var dbListCommand = new Command("list", "List all database connections");
dbListCommand.SetHandler(async () =>
{
    var dbService = serviceProvider.GetRequiredService<IDatabaseConnectionService>();
    var logger = serviceProvider.GetRequiredService<Serilog.ILogger>();
    
    try
    {
        var connections = await dbService.GetAllDatabaseConnectionsAsync();
        
        logger.Information("\nConfigured Database Connections:");
        logger.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        foreach (var conn in connections)
        {
            logger.Information("  {Name,-15} | {Engine,-12} | {Host}:{Port}/{DatabaseName}", 
                conn.Name, conn.Engine, conn.Host, conn.Port, conn.DatabaseName);
        }
        
        logger.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
    }
    catch (Exception ex)
    {
        logger.Error(ex, "✗ Error: {Message}", ex.Message);
        Environment.Exit(1);
    }
});

// db test
var dbTestCommand = new Command("test", "Test connection to a database");
var testNameOption = new Option<string>("--name", "Connection name") { IsRequired = true };
dbTestCommand.AddOption(testNameOption);

dbTestCommand.SetHandler(async (name) =>
{
    var dbService = serviceProvider.GetRequiredService<IDatabaseConnectionService>();
    var logger = serviceProvider.GetRequiredService<Serilog.ILogger>();
    
    try
    {
        logger.Information("Testing connection to '{Name}'... ", name);
        var success = await dbService.TestConnectionAsync(name);
        
        if (success)
        {
            logger.Information("✓ Connection successful");
        }
        else
        {
            logger.Warning("✗ Connection failed");
            Environment.Exit(1);
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "\n✗ Error: {Message}", ex.Message);
        Environment.Exit(1);
    }
}, testNameOption);

// db delete
var dbDeleteCommand = new Command("delete", "Delete a database connection");
var deleteNameOption = new Option<string>("--name", "Connection name") { IsRequired = true };
dbDeleteCommand.AddOption(deleteNameOption);

dbDeleteCommand.SetHandler(async (name) =>
{
    var dbService = serviceProvider.GetRequiredService<IDatabaseConnectionService>();
    var logger = serviceProvider.GetRequiredService<Serilog.ILogger>();
    
    try
    {
        var deleted = await dbService.DeleteDatabaseConnectionAsync(name);
        
        if (deleted)
        {
            logger.Information("✓ Database connection '{Name}' deleted successfully", name);
        }
        else
        {
            logger.Warning("✗ Database connection '{Name}' not found", name);
            Environment.Exit(1);
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "✗ Error: {Message}", ex.Message);
        Environment.Exit(1);
    }
}, deleteNameOption);

dbCommand.AddCommand(dbAddCommand);
dbCommand.AddCommand(dbListCommand);
dbCommand.AddCommand(dbTestCommand);
dbCommand.AddCommand(dbDeleteCommand);

// info command
var infoCommand = new Command("info", "Display configuration and paths information");
infoCommand.SetHandler(() =>
{
    var logger = serviceProvider.GetRequiredService<Serilog.ILogger>();
    
    // Replicate logic from AppDbContext.GetDefaultDbPath
    string configDbPath;
    var processPath = Environment.ProcessPath;
    if (!string.IsNullOrEmpty(processPath))
    {
        var processDir = Path.GetDirectoryName(processPath);
        if (!string.IsNullOrEmpty(processDir))
        {
            configDbPath = Path.Combine(processDir, "config.db");
        }
        else
        {
             var baseDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
             configDbPath = Path.Combine(baseDir, "config.db");
        }
    }
    else
    {
         var baseDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
         configDbPath = Path.Combine(baseDir, "config.db");
    }

    var logDir = Path.Combine(Path.GetDirectoryName(configDbPath) ?? AppContext.BaseDirectory, "logs");
    
    logger.Information("\nDbTool Configuration:");
    logger.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    logger.Information("  Executable:     {ProcessPath}", Environment.ProcessPath);
    logger.Information("  Config DB:      {ConfigDbPath}", configDbPath);
    logger.Information("  DB Exists:      {DbExists}", File.Exists(configDbPath));
    logger.Information("  Log Directory:  {LogDir}", logDir);
    logger.Information("  Version:        0.1.2");
    logger.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
});

dbCommand.AddCommand(infoCommand);

// Backup command
var backupCommand = new Command("backup", "Create a database backup");
var backupDbOption = new Option<string>("--db", "Database connection name") { IsRequired = true };
var backupOutputOption = new Option<string?>("--output", "Output directory (optional)");
backupCommand.AddOption(backupDbOption);
backupCommand.AddOption(backupOutputOption);

backupCommand.SetHandler(async (dbName, outputDir) =>
{
    var backupService = serviceProvider.GetRequiredService<IBackupService>();
    var logger = serviceProvider.GetRequiredService<Serilog.ILogger>();
    
    try
    {
        var progress = new Progress<string>(msg => logger.Information("{Message}", msg));
        var result = await backupService.CreateBackupAsync(dbName, outputDir, progress);
        
        if (result.Success)
        {
            logger.Information("\n✓ Backup completed successfully");
            logger.Information("  File: {FilePath}", result.FilePath);
            logger.Information("  Size: {SizeMB:F2} MB", result.FileSizeBytes / 1024.0 / 1024.0);
        }
        else
        {
            logger.Error("\n✗ Backup failed: {ErrorMessage}", result.ErrorMessage);
            Environment.Exit(1);
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "✗ Error: {Message}", ex.Message);
        Environment.Exit(1);
    }
}, backupDbOption, backupOutputOption);

// List backups command
var listBackupsCommand = new Command("list-backups", "List all backups for a database");
var listBackupsDbOption = new Option<string>("--db", "Database connection name") { IsRequired = true };
listBackupsCommand.AddOption(listBackupsDbOption);

listBackupsCommand.SetHandler(async (dbName) =>
{
    var backupService = serviceProvider.GetRequiredService<IBackupService>();
    var logger = serviceProvider.GetRequiredService<Serilog.ILogger>();
    
    try
    {
        var backups = await backupService.ListBackupsAsync(dbName);
        
        logger.Information("\nBackups for '{DbName}':", dbName);
        logger.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        foreach (var backup in backups)
        {
            var sizeMB = backup.FileSizeBytes / 1024.0 / 1024.0;
            logger.Information("  [{Status,-10}] {CreatedAt:yyyy-MM-dd HH:mm} | {SizeMB:F2} MB | {FilePath}", 
                backup.Status, backup.CreatedAt, sizeMB, backup.FilePath);
        }
        
        logger.Information("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
    }
    catch (Exception ex)
    {
        logger.Error(ex, "✗ Error: {Message}", ex.Message);
        Environment.Exit(1);
    }
}, listBackupsDbOption);

// Restore command
var restoreCommand = new Command("restore", "Restore a database from a backup file");
var restoreDbOption = new Option<string>("--db", "Database connection name") { IsRequired = true };
var restoreFileOption = new Option<string>("--file", "Path to backup file") { IsRequired = true };
var restoreForceOption = new Option<bool>("--force", "Skip confirmation prompt");
restoreCommand.AddOption(restoreDbOption);
restoreCommand.AddOption(restoreFileOption);
restoreCommand.AddOption(restoreForceOption);

restoreCommand.SetHandler(async (dbName, backupFile, force) =>
{
    var backupService = serviceProvider.GetRequiredService<IBackupService>();
    var logger = serviceProvider.GetRequiredService<Serilog.ILogger>();
    
    try
    {
        // Validate file exists
        if (!File.Exists(backupFile))
        {
            logger.Error("✗ Error: Backup file not found: {BackupFile}", backupFile);
            Environment.Exit(1);
            return;
        }

        // Safety confirmation
        if (!force)
        {
            logger.Warning("\n⚠️  WARNING: This will restore the database '{DbName}' from:", dbName);
            logger.Warning("   {BackupFile}", backupFile);
            logger.Warning("\n   This operation will overwrite existing data!");
            
            // Use Console.Write/ReadLine for interactive user input (not logging)
            Console.Write($"\nContinue? (yes/N): ");
            
            var response = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (response != "yes")
            {
                logger.Information("Restore cancelled.");
                Environment.Exit(0);
                return;
            }
        }

        var progress = new Progress<string>(msg => logger.Information("{Message}", msg));
        var result = await backupService.RestoreBackupAsync(dbName, backupFile, progress);
        
        if (result.Success)
        {
            logger.Information("\n✓ Restore completed successfully");
            logger.Information("  Database: {DatabaseName}", result.DatabaseName);
            logger.Information("  From: {BackupFilePath}", result.BackupFilePath);
        }
        else
        {
            logger.Error("\n✗ Restore failed: {ErrorMessage}", result.ErrorMessage);
            Environment.Exit(1);
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "✗ Error: {Message}", ex.Message);
        Environment.Exit(1);
    }
}, restoreDbOption, restoreFileOption, restoreForceOption);

rootCommand.AddCommand(dbCommand);
rootCommand.AddCommand(backupCommand);
rootCommand.AddCommand(listBackupsCommand);
rootCommand.AddCommand(restoreCommand);

return await rootCommand.InvokeAsync(args);
