namespace FYFY {
	public class ArgumentNullException : ExceptionWithCustomStackTrace {
		public ArgumentNullException(string stackTrace = null) : base("Argument cannot be null.", stackTrace) {
		}
	}
}