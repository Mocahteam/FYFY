using FYFY;

namespace FYFY_plugins.Monitoring {
    /// <summary>
    /// 	Exception used when the data given to build the trace are not matching any ComponentMonitoring or monitored Family.
    /// </summary>
    public class InvalidTraceException : ExceptionWithCustomStackTrace
    {
        /// <summary>
        /// 	Initializes a new instance of the <see cref="FYFY_plugins.Monitoring.InvalidTraceException"/> class.
        /// </summary>
        public InvalidTraceException(string stackTrace = null) : base("Unable to trace because arguments are invalid.", stackTrace)
        {
        }

        /// <summary>
        /// 	Initializes a new instance of the <see cref="FYFY_plugins.Monitoring.InvalidTraceException"/> class.
        /// </summary>
        public InvalidTraceException(string message, string stackTrace) : base(message, stackTrace)
        {
        }
    }
}