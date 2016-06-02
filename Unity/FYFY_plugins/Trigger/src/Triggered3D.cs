using UnityEngine;

namespace FYFY_plugins.Trigger {
	[DisallowMultipleComponent]
	[RequireComponent(typeof(TriggerSensitive3D))]
	[AddComponentMenu("")]
	public class Triggered3D : Triggered {
		private void Awake() {
			this._others = this.gameObject.GetComponent<TriggerSensitive3D>()._othersReadOnly;
		}
	}
}