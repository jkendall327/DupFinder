using Castle.MicroKernel.Registration;
using Castle.Windsor;
using DupFinderApp.ViewModels;
using DupFinderCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System.Windows;

namespace DupFinder
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ILogger log = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File(
                path: "log.txt",
                restrictedToMinimumLevel: LogEventLevel.Debug,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true)
            .CreateLogger();

            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfiguration Configuration = builder.Build();

            WindsorContainer ioc = BuildDIContainer(log, Configuration);
            var window = ioc.Resolve<MainWindow>();
            window.Show();
        }

        private static WindsorContainer BuildDIContainer(ILogger log, IConfiguration Configuration)
        {
            var ioc = new WindsorContainer();
            ioc.Register(Component.For<ILogger>().Instance(log));
            ioc.Register(Component.For<IConfiguration>().Instance(Configuration));

            ioc.Register(Component.For<IProcessor>().ImplementedBy<Processor>());

            ioc.Register(Component.For<IImageLoader>().ImplementedBy<ImageLoader>());
            ioc.Register(Component.For<IImageSetLoader>().ImplementedBy<ImageSetLoader>());
            ioc.Register(Component.For<IImageComparer>().ImplementedBy<ImageComparer>());
            ioc.Register(Component.For<IImageComparisonRuleset>().ImplementedBy<ImageComparisonRuleset>());
            ioc.Register(Component.For<IPairFinder>().ImplementedBy<PairFinder>());

            ioc.Register(Component.For<MainWindowViewModel>().ImplementedBy<MainWindowViewModel>());
            ioc.Register(Component.For<MainWindow>().ImplementedBy<MainWindow>());

            return ioc;
        }
    }
}
