namespace FYFY {
	/// <summary>
	/// 	Exception used when you try to access to a <c>GameObject</c> that is unknown to <c>FYFY</c>.
	/// </summary>
	public class UnknownGameObjectException : ExceptionWithCustomStackTrace {
		public UnknownGameObjectException(string stackTrace = null) : base("GameObject is unknown to FYFY but you are trying to access it with FYFY functions.", stackTrace) {
		}
	}
}