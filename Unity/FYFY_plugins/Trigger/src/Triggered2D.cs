using UnityEngine;

namespace FYFY_plugins.Trigger {
	[DisallowMultipleComponent]
	[RequireComponent(typeof(TriggerSensitive2D))]
	[AddComponentMenu("")]
	public class Triggered2D : Triggered {
		private void Awake() {
			this._others = this.gameObject.GetComponent<TriggerSensitive2D>()._othersReadOnly;
		}
	}
}