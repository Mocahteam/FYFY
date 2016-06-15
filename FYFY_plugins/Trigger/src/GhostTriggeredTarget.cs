using UnityEngine;

namespace FYFY_plugins.Trigger {
	[AddComponentMenu("")]
	[HideInInspector]
	public class GhostTriggeredTarget : MonoBehaviour {
		public TriggerSensitive _triggerSensitiveSource;

		private void OnDestroy(){ // check if you have to destroy the corresponding Triggered2D or Triggered3D
			if(_triggerSensitiveSource == null || _triggerSensitiveSource._destroying == true) { // triggerSensitive destroyed before gtt
				return;
			}
			
			if(_triggerSensitiveSource._targets.Remove(this.gameObject)) {
				if (_triggerSensitiveSource._targets.Count == 0) {
					_triggerSensitiveSource.detriggered (); // treated in the same preprocess than the source destroy of this callback
				}
			} else {
				// throw error !!
			}
		}
	}
}