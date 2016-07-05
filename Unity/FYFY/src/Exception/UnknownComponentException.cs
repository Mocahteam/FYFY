namespace FYFY {
	/// <summary>
	/// 	Exception used when you try to access to a <c>Component</c> that is unknown to <c>FYFY</c>.
	/// </summary>
	public class UnknownComponentException : ExceptionWithCustomStackTrace {
		public UnknownComponentException(string stackTrace = null) : base("Component is unknown to FYFY but you are trying to access it with FYFY functions.", stackTrace) {
		}
	}
}