using UnityEngine;

namespace FYFY_plugins.Trigger {
	//* <summary>
	//* 	Ghost triggered target.
	//* </summary>

	// TESTER EN LA METTANT INTERNAL ????????????????

	[AddComponentMenu("")]
	[HideInInspector]
	public class GhostTriggeredTarget : MonoBehaviour {
		/// <summary>
		/// The trigger sensitive source.
		/// </summary>
		public TriggerSensitive _triggerSensitiveSource;

		/// <summary>
		/// Raises the destroy event.
		/// </summary>
		private void OnDestroy(){ // check if you have to destroy the corresponding Triggered2D or Triggered3D
			Debug.Log("YEAH");

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