using log4net.helpers;
using log4net.spi;
using System;
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
        /// Gets or sets a semicolon-delimited list of recipient e-mail addresses.
        /// </summary>
        /// <value>A semicolon-delimited list of e-mail addresses.</value>
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
            using (var log = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(new LoggingLevelSwitch(logEventLevel))
                .WriteTo.Seq(_seqHost,apiKey: _apiKey)
                .CreateLogger())
            {
                try
                {
                    var hostName = SystemInfo.HostName;
                    for (var index = 0; index < events.Length; ++index)
                    {
                        if (events[index].Properties["log4net:HostName"] == null)
                            events[index].Properties["log4net:HostName"] = (object)hostName;

                        if (events[index].Level == Level.DEBUG)
                        {
                            log.Debug("{@event}", events[index]);
                        }
                        if (events[index].Level == Level.INFO)
                        {
                            log.Information("{@event}", events[index]);
                        }
                        if (events[index].Level == Level.WARN)
                        {
                            log.Warning("{@event}", events[index]);
                        }
                        if (events[index].Level == Level.ERROR)
                        {
                            log.Error("{@event}", events[index]);
                        }
                        if (events[index].Level == Level.FATAL)
                        {
                            log.Fatal("{@event}", events[index]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.ErrorHandler.Error("Error occurred while sending e-mail notification.", ex);
                }
            }
        }

        /// <summary>
        /// This appender requires a <see cref="N:log4net.Layout" /> to be set.
        /// </summary>
        /// <value><c>true</c></value>
        protected override bool RequiresLayout => true;
    }
}
