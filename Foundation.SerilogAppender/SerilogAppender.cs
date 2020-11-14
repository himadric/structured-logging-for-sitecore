using log4net.spi;
using System;
using log4net.helpers;
using Serilog;
using Serilog.Core;
using Serilog.Events;


namespace log4net.Appender
{
    public class SerilogAppender : BufferingAppenderSkeleton
    {
        private string _minimumLevel;
        private string _apiKey;
        private string _seqHost;

        /// <summary>
        /// Gets or sets minimum logging level.
        /// </summary>
        /// <value></value>
        public string MinimumLevel
        {
            get => this._minimumLevel;
            set => this._minimumLevel = value;
        }

        /// <summary>Gets or sets the API for Seq.</summary>
        /// <value>API Key</value>
        public string ApiKey
        {
            get => _apiKey;
            set => _apiKey = value;
        }

        /// <summary>
        /// Gets or sets Seq hostname
        /// </summary>
        /// <value>
        /// Seq host url
        /// </value>
        public string SeqHost
        {
            get => _seqHost;
            set => _seqHost = value;
        }

        /// <summary>Obselete</summary>
        /// <remarks>
        /// Use the BufferingAppenderSkeleton Fix methods instead
        /// </remarks>
        [Obsolete("Use the BufferingAppenderSkeleton Fix methods")]
        public bool LocationInfo
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        /// <summary>
        /// Sends the contents of the cyclic buffer as Seq events.
        /// </summary>
        /// <param name="events">The logging events to send.</param>
        protected override void SendBuffer(LoggingEvent[] events)
        {
            using (var log = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(new LoggingLevelSwitch(GetLogEventLevel()))
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithProperty("ThreadId", SystemInfo.CurrentThreadId)
                .Enrich.WithMemoryUsage()
                .WriteTo.Seq(_seqHost, apiKey: _apiKey)
                .CreateLogger())
            {
                foreach (var thisEvent in events)
                {
                    LogEvent(log, thisEvent);
                }
            }

        }

        /// <summary>
        /// This appender requires a <see cref="N:log4net.Layout" /> to be set.
        /// </summary>
        /// <value><c>true</c></value>
        protected override bool RequiresLayout => true;

        private void LogEvent(Logger log, LoggingEvent loggingEvent)
        {
            try
            {
                if (loggingEvent.Level == Level.DEBUG)
                {
                    log.Debug(loggingEvent.RenderedMessage);
                }
                if (loggingEvent.Level == Level.INFO)
                {
                    log.Information(loggingEvent.RenderedMessage);
                }
                if (loggingEvent.Level == Level.WARN)
                {
                    log.Warning(loggingEvent.RenderedMessage);
                }
                if (loggingEvent.Level == Level.ERROR)
                {
                    log.Error(loggingEvent.RenderedMessage);
                }
                if (loggingEvent.Level == Level.FATAL)
                {
                    log.Fatal(loggingEvent.RenderedMessage);
                }
            }
            catch (Exception ex)
            {
                this.ErrorHandler.Error("Error occurred while sending e-mail notification.", ex);
            }
        }
        private LogEventLevel GetLogEventLevel()
        {
            var logEventLevel = LogEventLevel.Debug;
            switch (MinimumLevel.ToLower())
            {
                case "debug":
                    logEventLevel = LogEventLevel.Debug;
                    break;
                case "info":
                    logEventLevel = LogEventLevel.Information;
                    break;
                case "warn":
                    logEventLevel = LogEventLevel.Warning;
                    break;
                case "error":
                    logEventLevel = LogEventLevel.Error;
                    break;
                case "fatal":
                    logEventLevel = LogEventLevel.Fatal;
                    break;
            }

            return logEventLevel;
        }
    }
}
