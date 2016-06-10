using UnityEngine;

namespace FYFY_plugins.Trigger {
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	[RequireComponent(typeof(TriggerSensitive3D))]
	public class Triggered3D : Triggered {
		private void Awake() {
			TriggerSensitive3D ts3D = this.gameObject.GetComponent<TriggerSensitive3D>();
			_targets = ts3D._targets;
		}
	}
}