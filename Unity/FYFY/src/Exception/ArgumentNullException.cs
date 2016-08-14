namespace FYFY {
	/// <summary>
	/// 	Exception used when a null reference is passed to a method that doesn't accept it as a valid argument.
	/// </summary>
	public class ArgumentNullException : ExceptionWithCustomStackTrace {
		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.ArgumentNullException"/> class.
		/// </summary>
		public ArgumentNullException(string stackTrace = null) : base("Argument cannot be null.", stackTrace) {
		}
	}
}