namespace FYFY {
	/// <summary>
	/// 	Exception with custom stack trace representation.
	/// </summary>
	/// <remarks>
	/// 	<para>By default, the stack trace is captured immediately before an exception object is thrown.</para>
	/// </remarks>
	public abstract class ExceptionWithCustomStackTrace : System.Exception {
		private readonly string _stackTrace;

		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.ExceptionWithCustomStackTrace"/> class.
		/// </summary>
		public ExceptionWithCustomStackTrace(string stackTrace) : base() {
			_stackTrace = stackTrace;
		}

		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.ExceptionWithCustomStackTrace"/> class.
		/// </summary>
		public ExceptionWithCustomStackTrace(string message, string stackTrace) : base(message) {
			_stackTrace = stackTrace;
		}

		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.ExceptionWithCustomStackTrace"/> class.
		/// </summary>
		public ExceptionWithCustomStackTrace(string message, System.Exception inner, string stackTrace) : base(message, inner) {
			_stackTrace = stackTrace;
		}

		/// <summary>
		/// 	Gets a string represention that describes the function calls that led up
		/// 	to the <c>Exception</c>.
		/// </summary>
		/// <remarks>
		/// 	The stack trace representation can be set in constructor to get customs 
		/// 	informations, otherwise, if it equals to null, the stack trace is captured 
		/// 	immediately before the exception object is thrown.
		/// </remarks>
		public override string StackTrace {
			get {
				return (_stackTrace != null) ? _stackTrace : base.StackTrace;
			}
		}
	}
}