using UnityEngine;

namespace FYFY_plugins.TriggerManager {
	/// <summary>
	/// 	Component specifying that the GameObject is in contact with at least one other GameObject.
	/// </summary>
	/// <remarks>
	/// 	<para>! AUTOMATICALLY ADDED, UPDATED OR REMOVED !</para>
	/// 	<para>! DO NOT TOUCH MANUALLY OTHERWISE WE CAN'T GUARANTEE THE COMPORTMENT !</para>
	/// </remarks>
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	public class Triggered3D : MonoBehaviour {
		private TriggerSensitive3D _triggerSensitive;

		private void Awake() {
			_triggerSensitive = this.gameObject.GetComponent<TriggerSensitive3D>();
		}

		/// <summary>
		/// 	Gets all the GameObjects in contact with this GameObject.
		/// </summary>
		public GameObject[] Targets {
			get {
				return _triggerSensitive.getTargets();
			}
		}
	}
}