using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CatLands.Hub.ViewModels;
using CatLands.Hub.Views;

namespace CatLands.Hub;

using System;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                //DataContext = new MainWindowViewModel(),
            };
            
            desktop.MainWindow.FindControl<OpenProjectView>("OpenProjectView")!.DataContext = new OpenProjectViewModel();
            desktop.MainWindow.FindControl<CreateProjectView>("CreateProjectView")!.DataContext = new CreateProjectViewModel();
            
            var services = new ServiceCollection();
            services.AddSingleton<IFilesService>(x => new FilesService(desktop.MainWindow));
            Services = services.BuildServiceProvider();
        }

        base.OnFrameworkInitializationCompleted();
    }
    public new static App? Current => Application.Current as App;

    public IServiceProvider? Services { get; private set; }
}