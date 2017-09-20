namespace FYFY {
	/// <summary>
	/// 	Default exception used when a problem occurs with <c>Fyfy</c>.
	/// </summary>
	public class FyfyException : ExceptionWithCustomStackTrace {
		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.FyfyException"/> class.
		/// </summary>
		public FyfyException(string stackTrace = null) : base("FYFY Exception.", stackTrace) {
		}

		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.FyfyException"/> class.
		/// </summary>
		public FyfyException(string message, string stackTrace): base(message, stackTrace){
		}
	}
}