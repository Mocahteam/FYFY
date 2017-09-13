using FYFY;

namespace monitoring {
	/// <summary>
	/// 	Exception used when links associated to an action don't match with logic expression of this action.
	/// </summary>
	public class TraceAborted : ExceptionWithCustomStackTrace {
		/// <summary>
		/// 	Initializes a new instance of the <see cref="monitoring.TraceAborted"/> class.
		/// </summary>
		public TraceAborted(): base("Check logic expression and links concerned.", null){
		}

		/// <summary>
		/// 	Initializes a new instance of the <see cref="monitoring.TraceAborted"/> class.
		/// </summary>
		public TraceAborted(string message, string stackTrace): base(message, stackTrace){
		}
	}
}