using FYFY;

namespace FYFY_plugins.Monitoring {
    /// <summary>
    /// 	Exception used when you try to access to a <c>GameObject</c> that is unknown to <c>FYFY</c>.
    /// </summary>
    public class InvalidTraceException : ExceptionWithCustomStackTrace
    {
        /// <summary>
        /// 	Initializes a new instance of the <see cref="FYFY.InvalidTraceException"/> class.
        /// </summary>
        public InvalidTraceException(string stackTrace = null) : base("Unable to trace because arguments are invalid.", stackTrace)
        {
        }

        /// <summary>
        /// 	Initializes a new instance of the <see cref="FYFY.FyfyException"/> class.
        /// </summary>
        public InvalidTraceException(string message, string stackTrace) : base(message, stackTrace)
        {
        }
    }
}