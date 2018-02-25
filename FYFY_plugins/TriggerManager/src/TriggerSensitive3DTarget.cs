using UnityEngine;
using FYFY;

namespace FYFY_plugins.TriggerManager {
	/// <summary></summary>

	// ! INVISIBLE FOR USERS IN INSPECTOR !

	// Component automatically added or removed.
	// A GameObject can have multiple components of this type: one for each GameObject touched
	// which has a TriggerSensitive3D component.

	//
	// Allow to set a gameObject as a target of a collision in order to solve a Unity problem:
	//		When a gameobject was deleted/disabled as it was a target of a collision (registered
	//		in the source), the collision exit event is not fired in the gameobject source, so
	//		we can't unregister it of the source.
	//		This component solves the problem by implementing an OnDestroy and OnDisable callback
	//		(called by Unity) to unregister it when it was destroyed/disabled.
	//
	[AddComponentMenu("")]
	public class TriggerSensitive3DTarget : MonoBehaviour {
		/// <summary></summary>

		// Setted in the source TriggerSensitive3D.OnTriggerEnter.
		public TriggerSensitive3D _source;

		// Hides the component in inspector.
		private void Awake() {
			this.hideFlags = HideFlags.HideInInspector;
		}

		// Unsubscribes the target (GameObject relative to this component) from the source if necessary.
		private void OnDestroy(){
			if(_source != null)
				_source.unregisterTarget(this.gameObject);
		}
		
		// Ask to destroy this component
		private void OnDisable() {
			Object.Destroy(this);
		}
	}
}