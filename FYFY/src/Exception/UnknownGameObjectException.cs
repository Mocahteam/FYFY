namespace FYFY {
	/// <summary>
	/// 	Exception used when you try to access to a <c>GameObject</c> that is unknown to <c>FYFY</c>.
	/// </summary>
	public class UnknownGameObjectException : ExceptionWithCustomStackTrace {
		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.UnknownGameObjectException"/> class.
		/// </summary>
		public UnknownGameObjectException(string stackTrace = null) : base("GameObject is unknown to FYFY but you are trying to access it with FYFY functions. Only a binded GameObject can be managed with Fyfy functions.", stackTrace) {
		}

		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.FyfyException"/> class.
		/// </summary>
		public UnknownGameObjectException(string message, string stackTrace): base(message, stackTrace){
		}
	}
}