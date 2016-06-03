using UnityEngine;

namespace FYFY_plugins.Trigger {
	[DisallowMultipleComponent]
	[RequireComponent(typeof(TriggerSensitive2D))]
	[AddComponentMenu("")]
	public class Triggered2D : Triggered {
		private void Awake() {
			TriggerSensitive2D ts2D = this.gameObject.GetComponent<TriggerSensitive2D>();
			this._others = ts2D._others;
			this._othersReadOnly = ts2D._othersReadOnly;
		}
	}
}