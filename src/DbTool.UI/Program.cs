using Avalonia;
using System;
using DbTool.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DbTool.UI;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        ConfigureServices();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static void ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure();
        App.ServiceProvider = services.BuildServiceProvider();
    }
}
