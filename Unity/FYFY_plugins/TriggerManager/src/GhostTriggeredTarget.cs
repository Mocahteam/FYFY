using UnityEngine;

namespace FYFY_plugins.TriggerManager {
	/// <summary></summary>

	// Component automatically added or removed.
	// INVISIBLE FOR USERS IN INSPECTOR BUT ACCESSIBLE VIA SOURCE CODE (BUT THEY CANT HAVE TO USE IT)

	// Allow to set a gameObject as a target of a collision in order to solve a Unity problem:
	//		When a gameobject was deleted as it was a target of a collision (so register in the source TriggerSensitive._targets dictionnary),
	// 		the triggered exit event is not fired in the gameobject source, so we can't	unregister it of the source.
	//		This component solves the problem by implementing an OnDestroy callback (called by Unity) to unregister it when it was destroyed.
	[AddComponentMenu("")]
	[HideInInspector]
	public class GhostTriggeredTarget : MonoBehaviour {
		/// <summary></summary>
		public TriggerSensitive _triggerSensitiveSource; // collision source

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