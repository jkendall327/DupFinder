using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace DupFinder
{
    /// <summary>
    /// Sink that hooks up to Serilog and dispatches its messages to the WPF UI.
    /// </summary>
    class UISink : ILogEventSink
    {
        readonly ITextFormatter _textFormatter = new MessageTemplateTextFormatter("{Timestamp} [{Level}] {Message}{Exception}");
        public ObservableCollection<string> UICollection { get; set; } = new();
        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            var renderSpace = new StringWriter();
            _textFormatter.Format(logEvent, renderSpace);

            // this is what gets the message back to WPF's UI thread -- would break thread affinity otherwise
            App.Current.Dispatcher.Invoke(() => UICollection.Add(renderSpace.ToString()));
        }
    }
}
