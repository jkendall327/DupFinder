using DupFinderApp;
using DupFinderApp.ViewModels;
using DupFinderCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Windows;

namespace DupFinder
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            BuildDIContainer().GetRequiredService<MainWindow>().Show();
        }

        private static IServiceProvider BuildDIContainer()
        {
            var ioc = new ServiceCollection();

            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            // for logging to the UI
            var sink = new UISink();

            ILogger log = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .WriteTo.Sink(sink)
                .CreateLogger();

            log.Information("Program started.");

            return ioc.AddSingleton(log)
            .AddSingleton(sink)

            .AddSingleton(config)

            .AddTransient<Processor>()
            .AddTransient<ImageSetLoader>()
            .AddTransient<PairFinder>()
            .AddTransient<IImageComparisonRuleset, ImageComparisonRuleset>()
            .AddTransient<IImageComparer, ImageComparer>()

            .AddSingleton<UserSettings>()
            .AddSingleton<OptionsViewModel>()
            .AddTransient<MainWindow>()
            .AddTransient<MainWindowViewModel>()

            .AddTransient(s =>
            {
                return new Func<OptionsView>(() =>
                {
                    OptionsViewModel vm = s.GetRequiredService<OptionsViewModel>();
                    return new OptionsView(vm) { DataContext = vm };
                });
            })

            .BuildServiceProvider();
        }
    }
}
