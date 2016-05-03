namespace UECS {
	public abstract class System {
		public bool Pause { get; set; }

		public abstract void process(int currentFrame);
	}
}