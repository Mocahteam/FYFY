namespace FYFY {
	public abstract class FSystem {
		private bool _pause;

		public bool Pause {
			get { 
				return _pause;
			}
			set {
				if (value != _pause) {
					_pause = value;
					if (value == false)
						this.onPause(UnityEngine.Time.frameCount);
					else
						this.onResume(UnityEngine.Time.frameCount);
				}
			}
		}

		protected abstract void onPause(int currentFrame);
		protected abstract void onResume(int currentFrame);
		protected abstract void onProcess(int currentFrame);

		internal void process(int currentFrame) {
			this.onProcess(currentFrame);
		}
	}
}