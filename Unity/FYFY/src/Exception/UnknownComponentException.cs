namespace FYFY {
	/// <summary>
	/// 	Exception used when you try to access to a <c>Component</c> that is unknown to <c>FYFY</c>.
	/// </summary>
	public class UnknownComponentException : ExceptionWithCustomStackTrace {
		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.UnknownComponentException"/> class.
		/// </summary>
		public UnknownComponentException(string stackTrace = null) : base("Component is unknown to FYFY but you are trying to access it with FYFY functions.", stackTrace) {
		}
	}
}