namespace FYFY {
	/// <summary>
	/// 	Exception used when you try to access to a <c>GameObject</c> that has been destroyed.
	/// </summary>
	public class DestroyedGameObjectException : ExceptionWithCustomStackTrace {
		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.DestroyedGameObjectException"/> class.
		/// </summary>
		public DestroyedGameObjectException(string stackTrace = null) : base("GameObject has been destroyed but you are still trying to access it.", stackTrace) {
		}
	}
}