namespace FYFY {
	public class DestroyedGameObjectException : ExceptionWithCustomStackTrace {
		public DestroyedGameObjectException(string stackTrace = null) : base("GameObject has been destroyed but you are still trying to access it.", stackTrace) {
		}
	}
}