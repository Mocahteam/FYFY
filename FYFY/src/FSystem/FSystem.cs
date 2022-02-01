using System.Diagnostics;

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
		/// 	Gets the average execution time taken by onProcess
		/// </summary>
		public double avgExecDuration = 0;
		/// <summary>
		/// 	Gets the maximum execution time taken by onProcess
		/// </summary>
		public double maxExecDuration = 0;
		/// <summary>
		/// 	Show families used in this system inside Unity Inspector
		/// </summary>
		public bool showFamilies = false;
		
		private bool started = false;
		private double _accumulatedExecDuration = 0;
		private int _execDurationCount = 0;
		private Stopwatch _stopwatch = new Stopwatch ();

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
					
					if (started){
						if (value) {
							this.onPause(UnityEngine.Time.frameCount);
						} else {
							this.onResume(UnityEngine.Time.frameCount);
						}
					}
					
					if (MainLoop.instance)
						MainLoop.instance._forceUpdateInspector++;
				}
			}
		}

		/// <summary>
		/// 	Function called when this <see cref="FYFY.FSystem"/> started. All families are populated and can be parsed.
		/// </summary>
		protected virtual void onStart(){
		}
		
		internal void start() {
			this.onStart();
			started = true;
		}
		
		/// <summary>
		/// 	Function called when this <see cref="FYFY.FSystem"/> paused.
		/// </summary>
		/// <param name="currentFrame">
		/// 	The <c>Unity</c> frame number when this function is called.
		/// </param>
		protected virtual void onPause(int currentFrame){
		}
		/// <summary>
		/// 	Function called when this <see cref="FYFY.FSystem"/> resumed.
		/// </summary>
		/// <param name="currentFrame">
		/// 	The <c>Unity</c> frame number when this function is called.
		/// </param>
		protected virtual void onResume(int currentFrame){
		}
		/// <summary>
		/// 	Function called each time when FYFY enter in the update block where this <see cref="FYFY.FSystem"/> is.
		/// </summary>
		/// <remarks>
		/// 	Called only if this <see cref="FYFY.FSystem"/> is active.
		/// </remarks>
		/// <param name="familiesUpdateCount">
		/// 	Number of times the families have been updated.
		/// </param>
		protected virtual void onProcess(int familiesUpdateCount){
		}

		internal void process(int familiesUpdateCount) {
			_stopwatch.Reset ();
			_stopwatch.Start ();
			this.onProcess(familiesUpdateCount);
			_stopwatch.Stop ();
			double duration = _stopwatch.ElapsedMilliseconds;
			if (maxExecDuration < duration)
				maxExecDuration = duration;
			_accumulatedExecDuration += duration;
			_execDurationCount += 1;
			avgExecDuration = _accumulatedExecDuration / _execDurationCount;
		}
	}
}