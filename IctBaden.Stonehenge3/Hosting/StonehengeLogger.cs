using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IctBaden.Stonehenge3.Hosting
{
    public static class StonehengeLogger
    {
        private static ILoggerFactory _defaultFactory;
        
        /// <summary>
        /// Tries to find static field of type ILoggerFactory in entry assembly.
        /// If not found return simple factory for console and tron output with default configuration.
        /// </summary>
        public static ILoggerFactory DefaultFactory
        {
            get
            {
                if (_defaultFactory != null) return _defaultFactory;
                
                // find static field of type ILoggerFactory in entry assembly
                var entry = Assembly.GetEntryAssembly();
                foreach (var entryType in entry!.DefinedTypes)
                {
                    var fieldInfo = entryType.DeclaredFields.FirstOrDefault(f => f.FieldType == typeof(ILoggerFactory));
                    var loggerFactory = (ILoggerFactory) fieldInfo?.GetValue(null);
                    if (loggerFactory == null) continue;
                    
                    Trace.TraceInformation($"Using LoggerFactory '{fieldInfo.Name}' of type '{entryType.Name}'.");
                    return loggerFactory;
                }

                Trace.TraceWarning($"No LoggerFactory found. Using console factory.");
                var emptyConfiguration = new ConfigurationBuilder().Build();
                _defaultFactory = CreateConsoleAndTronFactory(emptyConfiguration);
                return _defaultFactory;
            }
        }

        public static ILogger DefaultLogger => DefaultFactory.CreateLogger("stonehenge");

        public static LogLevel DefaultLevel = LogLevel.Warning;
        
        private static ILoggerFactory CreateConsoleAndTronFactory(IConfiguration configuration)
        {
            var minimumLogLevel = configuration.GetValue("LogLevel", DefaultLevel);
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(new TraceLoggerProvider("stonehenge"));
                builder.SetMinimumLevel(minimumLogLevel);
            });
            return loggerFactory;
        }

    }
    
    
    internal class TraceLogger : ILogger
    {
        private readonly string _context;
        private string _scopeContext = "";
    
        private class LogScope : IDisposable
        {
            private readonly TraceLogger _logger;
        
            public LogScope(TraceLogger logger, string context)
            {
                _logger = logger;
                _logger._scopeContext = context;
            }

            public void Dispose()
            {
                _logger._scopeContext = "";
            }
        }

        public TraceLogger(string context)
        {
            _context = context;
        }
        
        public IDisposable BeginScope<TState>(TState state)
        {
            return new LogScope(this, state.ToString());
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                var logLine = _context + ": ";
                if (!string.IsNullOrEmpty(_scopeContext))
                {
                    logLine += _scopeContext + " ";
                }
                logLine += state.ToString();
                if (exception != null)
                {
                    logLine += ", " + exception.Message;
                }
                switch (logLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                    case LogLevel.Information:
                        Trace.TraceInformation(logLine);
                        break;
                    case LogLevel.Warning:
                        Trace.TraceWarning(logLine);
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        Trace.TraceError(logLine);
                        break;
                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

    }
  
    internal class TraceLoggerProvider : ILoggerProvider
    {
        private readonly string _context;

        private readonly ConcurrentDictionary<string, TraceLogger> _loggers =
            new ConcurrentDictionary<string, TraceLogger>();

        public TraceLoggerProvider(string context)
        {
            _context = context;
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new TraceLogger(_context));

        public void Dispose() => _loggers.Clear();
    }

}