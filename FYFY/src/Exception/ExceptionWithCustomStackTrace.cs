namespace FYFY {
	public abstract class ExceptionWithCustomStackTrace : System.Exception {
		private readonly string _stackTrace;

		public ExceptionWithCustomStackTrace(string stackTrace) : base() {
			_stackTrace = stackTrace;
		}

		public ExceptionWithCustomStackTrace(string message, string stackTrace) : base(message) {
			_stackTrace = stackTrace;
		}

		public ExceptionWithCustomStackTrace(string message, System.Exception inner, string stackTrace) : base(message, inner) {
			_stackTrace = stackTrace;
		}

		public override string StackTrace {
			get {
				return (_stackTrace != null) ? _stackTrace : base.StackTrace;
			}
		}
	}
}