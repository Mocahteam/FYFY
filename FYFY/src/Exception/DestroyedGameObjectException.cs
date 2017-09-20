namespace FYFY {
	/// <summary>
	/// 	Exception used when you try to access to a <c>GameObject</c> that has been destroyed.
	/// </summary>
	public class DestroyedGameObjectException : ExceptionWithCustomStackTrace {
		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.DestroyedGameObjectException"/> class.
		/// </summary>
		public DestroyedGameObjectException(string stackTrace = null) : base("GameObject will be destroy during this frame and this action will not be possible. In a same frame, your must not destroy a GameObject and ask Fyfy to perform an action on it.", stackTrace) {
		}

		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.FyfyException"/> class.
		/// </summary>
		public DestroyedGameObjectException(string message, string stackTrace): base(message, stackTrace){
		}
	}
}