namespace FYFY {
	/// <summary>
	/// 	Exception used when you try to access to a <c>Component</c> that has been destroyed.
	/// </summary>
	public class DestroyedComponentException : ExceptionWithCustomStackTrace {
		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.DestroyedComponentException"/> class.
		/// </summary>
		public DestroyedComponentException(string stackTrace = null) : base("Component will be destroy during this frame and this action will not be possible. In a same frame, your must not destroy a Component and ask Fyfy to perform an action on it.", stackTrace) {
		}
		
		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.DestroyedComponentException"/> class.
		/// </summary>
		public DestroyedComponentException(string message, string stackTrace): base(message, stackTrace){
		}
	}
}