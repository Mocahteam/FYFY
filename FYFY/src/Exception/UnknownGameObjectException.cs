namespace FYFY {
	public class UnknownGameObjectException : ExceptionWithCustomStackTrace {
		public UnknownGameObjectException(string stackTrace = null) : base("GameObject is unknown to FYFY but you are trying to access it with FYFY functions.", stackTrace) {
		}
	}
}