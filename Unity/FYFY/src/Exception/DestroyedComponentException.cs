namespace FYFY {
	/// <summary>
	/// 	Exception used when you try to access to a <c>Component</c> that has been destroyed.
	/// </summary>
	public class DestroyedComponentException : ExceptionWithCustomStackTrace {
		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.DestroyedComponentException"/> class.
		/// </summary>
		public DestroyedComponentException(string stackTrace = null) : base("Component has been destroyed but you are still trying to access it.", stackTrace) {
		}
	}
}