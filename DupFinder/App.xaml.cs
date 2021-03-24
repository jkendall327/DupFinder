using Castle.Windsor;
using DupFinderCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Windows;

namespace DupFinder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ILogger log = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File(path: "log.txt", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            .CreateLogger();

            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfiguration Configuration = builder.Build();

            var ioc = new WindsorContainer();
            ioc.Register(Castle.MicroKernel.Registration.Component.For<ILogger>().Instance(log));
            ioc.Register(Castle.MicroKernel.Registration.Component.For<IConfiguration>().Instance(Configuration));

            ioc.Register(Castle.MicroKernel.Registration.Component.For<IProcessor>().ImplementedBy<Processor>());

            ioc.Register(Castle.MicroKernel.Registration.Component.For<IImageLoader>().ImplementedBy<ImageLoader>());
            ioc.Register(Castle.MicroKernel.Registration.Component.For<IImageSetLoader>().ImplementedBy<ImageSetLoader>());

            ioc.Register(Castle.MicroKernel.Registration.Component.For<MainWindow>().ImplementedBy<MainWindow>());

            var window = ioc.Resolve<MainWindow>();
            window.Show();
        }
    }
}
