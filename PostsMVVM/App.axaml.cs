using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PostsMVVM.Services;
using PostsMVVM.ViewModels;
using PostsMVVM.Views;

namespace PostsMVVM;

public partial class App : Application
{
    public override void Initialize()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log("UnhandledException", e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException     += (_, e) => { Log("UnobservedTaskException", e.Exception); e.SetObserved(); };
        AvaloniaXamlLoader.Load(this);
    }

    private static void Log(string source, Exception? ex) =>
        Debug.WriteLine($"[{source}] {ex}");

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddHttpClient<PostsService>(c =>
            c.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/"));
        services.AddTransient<MainViewModel>();
        var provider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}