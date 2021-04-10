using Castle.MicroKernel.Registration;
using Castle.Windsor;
using DupFinderApp.ViewModels;
using DupFinderCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Windows;

namespace DupFinder
{
    class InMemorySink : ILogEventSink
    {
        readonly ITextFormatter _textFormatter = new MessageTemplateTextFormatter("{Timestamp} [{Level}] {Message}{Exception}");

        public ConcurrentQueue<string> Events { get; } = new ConcurrentQueue<string>();

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            var renderSpace = new StringWriter();
            _textFormatter.Format(logEvent, renderSpace);
            Events.Enqueue(renderSpace.ToString());
        }
    }

    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfiguration Configuration = builder.Build();

            var sink = new InMemorySink();
            ILogger log = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteTo.Sink(sink)
                .CreateLogger();

            log.Information("Program started.");

            WindsorContainer ioc = BuildDIContainer(log, sink.Events, Configuration);
            var window = ioc.Resolve<MainWindow>();
            window.Show();
        }

        private static WindsorContainer BuildDIContainer(ILogger log, ConcurrentQueue<string> uiLogger, IConfiguration Configuration)
        {
            var ioc = new WindsorContainer();
            ioc.Register(Component.For<ILogger>().Instance(log));
            ioc.Register(Component.For<ConcurrentQueue<string>>().Instance(uiLogger));
            ioc.Register(Component.For<IConfiguration>().Instance(Configuration));

            ioc.Register(Component.For<IProcessor>().ImplementedBy<Processor>());

            ioc.Register(Component.For<IImageSetLoader>().ImplementedBy<ImageSetLoader>());
            ioc.Register(Component.For<IImageComparer>().ImplementedBy<ImageComparer>());
            ioc.Register(Component.For<IImageComparisonRuleset>().ImplementedBy<ImageComparisonRuleset>());
            ioc.Register(Component.For<IPairFinder>().ImplementedBy<PairFinder>());

            ioc.Register(Component.For<UserSettings>().ImplementedBy<UserSettings>());
            ioc.Register(Component.For<OptionsViewModel>().ImplementedBy<OptionsViewModel>());

            ioc.Register(Component.For<MainWindowViewModel>().ImplementedBy<MainWindowViewModel>());
            ioc.Register(Component.For<MainWindow>().ImplementedBy<MainWindow>());

            return ioc;
        }
    }
}
