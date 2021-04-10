using Castle.MicroKernel.Registration;
using Castle.Windsor;
using DupFinderApp.ViewModels;
using DupFinderCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows;

namespace DupFinder
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfiguration Configuration = builder.Build();

            // for logging to the UI
            var sink = new UISink();

            ILogger log = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteTo.Sink(sink)
                .CreateLogger();

            log.Information("Program started.");

            WindsorContainer ioc = BuildDIContainer(log, sink.UICollection, Configuration);
            var window = ioc.Resolve<MainWindow>();
            window.Show();
        }

        private static WindsorContainer BuildDIContainer(ILogger log, ObservableCollection<string> uiLogger, IConfiguration Configuration)
        {
            var ioc = new WindsorContainer();
            ioc.Register(Component.For<ILogger>().Instance(log));
            ioc.Register(Component.For<ObservableCollection<string>>().Instance(uiLogger));
            ioc.Register(Component.For<IConfiguration>().Instance(Configuration));

            ioc.Register(Component.For<Processor>().ImplementedBy<Processor>());

            ioc.Register(Component.For<ImageSetLoader>().ImplementedBy<ImageSetLoader>());
            ioc.Register(Component.For<IImageComparer>().ImplementedBy<ImageComparer>());
            ioc.Register(Component.For<IImageComparisonRuleset>().ImplementedBy<ImageComparisonRuleset>());
            ioc.Register(Component.For<PairFinder>().ImplementedBy<PairFinder>());

            ioc.Register(Component.For<UserSettings>().ImplementedBy<UserSettings>());
            ioc.Register(Component.For<OptionsViewModel>().ImplementedBy<OptionsViewModel>());

            ioc.Register(Component.For<MainWindowViewModel>().ImplementedBy<MainWindowViewModel>());
            ioc.Register(Component.For<MainWindow>().ImplementedBy<MainWindow>());

            return ioc;
        }
    }
}
