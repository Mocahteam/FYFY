using UnityEngine;

namespace FYFY_plugins.Trigger {
	[DisallowMultipleComponent]
	[RequireComponent(typeof(TriggerSensitive3D))]
	[AddComponentMenu("")]
	public class Triggered3D : Triggered {
		private void Awake() {
			TriggerSensitive3D ts3D = this.gameObject.GetComponent<TriggerSensitive3D>();
			this._others = ts3D._others;
			this._othersReadOnly = ts3D._othersReadOnly;
		}
	}
}