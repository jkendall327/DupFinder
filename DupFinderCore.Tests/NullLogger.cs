using Serilog.Events;

namespace DupFinderCore.Tests
{
    public class NullLogger : Serilog.ILogger
    {
        public void Write(LogEvent logEvent)
        {
            // ...
        }
    }
}
