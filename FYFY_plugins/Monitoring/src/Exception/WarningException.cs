using FYFY;

namespace FYFY_plugins.Monitoring {
    /// <summary>
    /// 	Exception used to build a clickable warning message.
    /// </summary>
    public class WarningException : ExceptionWithCustomStackTrace
    {
        /// <summary>
        /// 	Initializes a new instance of the <see cref="FYFY_plugins.Monitoring.WarningException"/> class.
        /// </summary>
        public WarningException(string stackTrace = null) : base("Warning!!!", stackTrace)
        {
        }

        /// <summary>
        /// 	Initializes a new instance of the <see cref="FYFY_plugins.Monitoring.WarningException"/> class.
        /// </summary>
        public WarningException(string message, string stackTrace) : base(message, stackTrace)
        {
        }
    }
}