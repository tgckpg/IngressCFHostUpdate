using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Reference: https://docs.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider
namespace IngressCFHostUpdate.Logging
{
    public class ColorConsoleLoggerConfiguration
    {
        public int EventId { get; set; }
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public ConsoleColor Color { get; set; } = ConsoleColor.Green;
    }

    public class ColorConsoleLogger : ILogger
    {
        private readonly string _name;
        private readonly ColorConsoleLoggerConfiguration _config;

        public ColorConsoleLogger( string name, ColorConsoleLoggerConfiguration config )
            => (_name, _config) = (name, config);

        public IDisposable BeginScope<TState>( TState state ) => default;

        public bool IsEnabled( LogLevel logLevel )
            => logLevel == _config.LogLevel;

        public void Log<TState>( LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter )
        {
            if ( !IsEnabled( logLevel ) )
                return;

            if ( _config.EventId == 0 || _config.EventId == eventId.Id )
            {
                ConsoleColor originalColor = Console.ForegroundColor;

                Console.ForegroundColor = _config.Color;
                Console.Write( $"[{eventId.Id}:{logLevel}]" );
                Console.ForegroundColor = originalColor;

                Console.WriteLine( $" {_name} - {formatter( state, exception )}" );
            }
        }
    }

    public sealed class ColorConsoleLoggerProvider : ILoggerProvider
    {
        private readonly ColorConsoleLoggerConfiguration _config;
        private readonly ConcurrentDictionary<string, ColorConsoleLogger> _loggers
            = new();

        public ColorConsoleLoggerProvider( ColorConsoleLoggerConfiguration config )
            => _config = config;

        public ILogger CreateLogger( string categoryName )
            => _loggers.GetOrAdd( categoryName, name => new ColorConsoleLogger( name, _config ) );

        public void Dispose()
            => _loggers.Clear();
    }

}
