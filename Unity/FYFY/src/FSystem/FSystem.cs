namespace FYFY {
	/// <summary>
	/// 	Base class every FYFY system derives from.
	/// </summary>
	/// <remarks>
	/// 	FYFY systems have to be setted in the <c>Unity Editor</c> in a MainLoop's block of update.
	/// </remarks>
	public abstract class FSystem {
		private bool? _pause; // bool default == 0 -> donc sinon le setter n'est appele a linit que de la cas on set a true (true != false)

		/// <summary>
		/// 	Gets or sets a value indicating whether this <see cref="FYFY.FSystem"/> is paused.
		/// </summary>
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

		/// <summary>
		/// 	Function called when this <see cref="FYFY.FSystem"/> paused.
		/// </summary>
		/// <param name="currentFrame">
		/// 	The <c>Unity</c> frame number when this function is called.
		/// </param>
		protected void onPause(int currentFrame){
		}
		/// <summary>
		/// 	Function called when this <see cref="FYFY.FSystem"/> resumed.
		/// </summary>
		/// <param name="currentFrame">
		/// 	The <c>Unity</c> frame number when this function is called.
		/// </param>
		protected void onResume(int currentFrame){
		}
		/// <summary>
		/// 	Function called each time when FYFY enter in the update block where this <see cref="FYFY.FSystem"/> is.
		/// </summary>
		/// <remarks>
		/// 	Called only is this <see cref="FYFY.FSystem"/> is active.
		/// </remarks>
		/// <param name="familiesUpdateCount">
		/// 	Number of times the families have been updated.
		/// </param>
		protected void onProcess(int familiesUpdateCount){
		}

		internal void process(int familiesUpdateCount) {
			this.onProcess(familiesUpdateCount);
		}
	}
}