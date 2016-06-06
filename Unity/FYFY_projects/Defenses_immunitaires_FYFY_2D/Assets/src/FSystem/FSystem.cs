namespace FYFY {
	public abstract class FSystem {
		private bool? _pause; // null for initialization because bool default == 0 -> donc sinon le setter n'est appele a linit quand de la cas on set a true (true != false)

		public bool Pause {
			get {
				return (_pause == null) ? false : (bool)_pause;
			}
			set {
				if(value != _pause) {
					_pause = value;
					if(value == false)
						this.onResume(UnityEngine.Time.frameCount);
					else
						this.onPause(UnityEngine.Time.frameCount);
				}
			}
		}

		protected abstract void onPause(int currentFrame);
		protected abstract void onResume(int currentFrame);
		protected abstract void onProcess(int familiesUpdateCount);

		internal void process(int familiesUpdateCount) {
			this.onProcess(familiesUpdateCount);
		}
	}
}