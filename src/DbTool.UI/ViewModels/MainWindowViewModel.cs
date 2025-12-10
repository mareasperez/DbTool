using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DbTool.Application.DTOs;
using DbTool.Application.Interfaces;

namespace DbTool.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDatabaseConnectionService _dbConnectionService;
    private readonly IBackupService _backupService;

    [ObservableProperty]
    private ObservableCollection<DatabaseConnectionDto> _connections = new();

    [ObservableProperty]
    private DatabaseConnectionDto? _selectedConnection;

    [ObservableProperty]
    private ObservableCollection<BackupInfoDto> _backups = new();

    [ObservableProperty]
    private BackupInfoDto? _selectedBackup;

    // Connection form fields
    [ObservableProperty]
    private string _connectionName = string.Empty;

    [ObservableProperty]
    private string _selectedEngine = "postgres";

    [ObservableProperty]
    private string _host = "localhost";

    [ObservableProperty]
    private int _port = 5432;

    [ObservableProperty]
    private string _databaseName = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _outputDirectory = string.Empty;

    [ObservableProperty]
    private string _restoreFilePath = string.Empty;

    public ObservableCollection<string> AvailableEngines { get; } = new()
    {
        "postgres",
        "mysql",
        "sqlserver",
        "mariadb"
    };

    public MainWindowViewModel(IDatabaseConnectionService dbConnectionService, IBackupService backupService)
    {
        _dbConnectionService = dbConnectionService;
        _backupService = backupService;
        
        // Load initial data - wrapped to handle exceptions
        Task.Run(async () =>
        {
            try
            {
                await LoadConnectionsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading initial data: {ex.Message}";
            }
        });
    }

    [RelayCommand]
    private async Task LoadConnectionsAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading connections...";
            var connections = await _dbConnectionService.GetAllDatabaseConnectionsAsync();
            Connections.Clear();
            foreach (var conn in connections)
            {
                Connections.Add(conn);
            }
            StatusMessage = $"Loaded {Connections.Count} connection(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading connections: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddConnectionAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ConnectionName) || string.IsNullOrWhiteSpace(DatabaseName))
            {
                StatusMessage = "Connection name and database name are required";
                return;
            }

            IsBusy = true;
            StatusMessage = "Adding connection...";

            var dto = new CreateDatabaseConnectionDto(
                ConnectionName,
                SelectedEngine,
                Host,
                Port,
                DatabaseName,
                Username,
                Password
            );

            await _dbConnectionService.CreateDatabaseConnectionAsync(dto);
            StatusMessage = $"Connection '{ConnectionName}' added successfully";

            // Clear form
            ConnectionName = string.Empty;
            DatabaseName = string.Empty;
            Username = string.Empty;
            Password = string.Empty;

            await LoadConnectionsAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding connection: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (SelectedConnection == null)
        {
            StatusMessage = "Please select a connection to test";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = $"Testing connection '{SelectedConnection.Name}'...";
            var success = await _dbConnectionService.TestConnectionAsync(SelectedConnection.Name);
            StatusMessage = success 
                ? $"✓ Connection '{SelectedConnection.Name}' successful" 
                : $"✗ Connection '{SelectedConnection.Name}' failed";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error testing connection: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteConnectionAsync()
    {
        if (SelectedConnection == null)
        {
            StatusMessage = "Please select a connection to delete";
            return;
        }

        try
        {
            IsBusy = true;
            var connectionName = SelectedConnection.Name;
            StatusMessage = $"Deleting connection '{connectionName}'...";
            var deleted = await _dbConnectionService.DeleteDatabaseConnectionAsync(connectionName);
            
            if (deleted)
            {
                StatusMessage = $"Connection '{connectionName}' deleted successfully";
                await LoadConnectionsAsync();
                SelectedConnection = null;
            }
            else
            {
                StatusMessage = $"Connection '{connectionName}' not found";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting connection: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateBackupAsync()
    {
        if (SelectedConnection == null)
        {
            StatusMessage = "Please select a connection to backup";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = $"Creating backup for '{SelectedConnection.Name}'...";

            var progress = new Progress<string>(msg => StatusMessage = msg);
            var result = await _backupService.CreateBackupAsync(
                SelectedConnection.Name,
                string.IsNullOrWhiteSpace(OutputDirectory) ? null : OutputDirectory,
                progress
            );

            if (result.Success)
            {
                var sizeMB = result.FileSizeBytes / 1024.0 / 1024.0;
                StatusMessage = $"✓ Backup completed: {result.FilePath} ({sizeMB:F2} MB)";
                await LoadBackupsAsync();
            }
            else
            {
                StatusMessage = $"✗ Backup failed: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating backup: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadBackupsAsync()
    {
        if (SelectedConnection == null)
        {
            Backups.Clear();
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = $"Loading backups for '{SelectedConnection.Name}'...";
            var backups = await _backupService.ListBackupsAsync(SelectedConnection.Name);
            Backups.Clear();
            foreach (var backup in backups.OrderByDescending(b => b.CreatedAt))
            {
                Backups.Add(backup);
            }
            StatusMessage = $"Loaded {Backups.Count} backup(s) for '{SelectedConnection.Name}'";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading backups: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RestoreBackupAsync()
    {
        if (SelectedConnection == null)
        {
            StatusMessage = "Please select a connection to restore";
            return;
        }

        if (string.IsNullOrWhiteSpace(RestoreFilePath))
        {
            StatusMessage = "Please specify a backup file to restore";
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = $"Restoring backup to '{SelectedConnection.Name}'...";

            var progress = new Progress<string>(msg => StatusMessage = msg);
            var result = await _backupService.RestoreBackupAsync(
                SelectedConnection.Name,
                RestoreFilePath,
                progress
            );

            if (result.Success)
            {
                StatusMessage = $"✓ Restore completed successfully";
            }
            else
            {
                StatusMessage = $"✗ Restore failed: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error restoring backup: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedConnectionChanged(DatabaseConnectionDto? value)
    {
        if (value != null)
        {
            Task.Run(async () =>
            {
                try
                {
                    await LoadBackupsAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error loading backups: {ex.Message}";
                }
            });
        }
    }

    partial void OnSelectedEngineChanged(string value)
    {
        // Update default port based on engine
        Port = value switch
        {
            "postgres" => 5432,
            "mysql" => 3306,
            "mariadb" => 3306,
            "sqlserver" => 1433,
            _ => Port
        };
    }
}
