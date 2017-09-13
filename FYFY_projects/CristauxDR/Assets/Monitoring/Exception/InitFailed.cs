using FYFY;

namespace monitoring {
	/// <summary>
	/// 	Exception used when links associated to an action don't match with logic expression of this action.
	/// </summary>
	public class InitFailed : ExceptionWithCustomStackTrace {
		/// <summary>
		/// 	Initializes a new instance of the <see cref="monitoring.InitFail"/> class.
		/// </summary>
		public InitFailed(): base("Initialization failed.", null){
		}

		/// <summary>
		/// 	Initializes a new instance of the <see cref="monitoring.InitFail"/> class.
		/// </summary>
		public InitFailed(string message, string stackTrace): base(message, stackTrace){
		}
	}
}