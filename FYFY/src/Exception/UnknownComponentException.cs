namespace FYFY {
	public class UnknownComponentException : ExceptionWithCustomStackTrace {
		public UnknownComponentException(string stackTrace = null) : base("Component is unknown to FYFY but you are trying to access it with FYFY functions.", stackTrace) {
		}
	}
}