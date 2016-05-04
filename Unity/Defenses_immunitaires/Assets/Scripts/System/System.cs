namespace UECS {
	public abstract class System {
		public bool Pause { get; set; }

		public abstract void process(int currentFrame);

		// ensure no more than 1 singleton by subclass
	}
}




//public System () {
//	UnityEngine.Debug.Log(this.GetType());
//}