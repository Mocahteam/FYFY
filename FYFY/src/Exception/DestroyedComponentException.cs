namespace FYFY {
	public class DestroyedComponentException : ExceptionWithCustomStackTrace {
		public DestroyedComponentException(string stackTrace = null) : base("Component has been destroyed but you are still trying to access it.", stackTrace) {
		}
	}
}