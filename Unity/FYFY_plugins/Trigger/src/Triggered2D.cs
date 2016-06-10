using UnityEngine;

namespace FYFY_plugins.Trigger {
	[DisallowMultipleComponent]
	[AddComponentMenu("")]
	[RequireComponent(typeof(TriggerSensitive2D))]
	public class Triggered2D : Triggered {
		private void Awake() {
			TriggerSensitive2D ts2D = this.gameObject.GetComponent<TriggerSensitive2D>();
			_targets = ts2D._targets;
		}
	}
}