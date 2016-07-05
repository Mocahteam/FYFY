namespace FYFY {
	/// <summary>
	/// 	Exception used when you try to access to a <c>Component</c> that has been destroyed.
	/// </summary>
	public class DestroyedComponentException : ExceptionWithCustomStackTrace {
		public DestroyedComponentException(string stackTrace = null) : base("Component has been destroyed but you are still trying to access it.", stackTrace) {
		}
	}
}