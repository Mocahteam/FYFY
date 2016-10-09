using UnityEngine;
using FYFY;

namespace FYFY_plugins.TriggerManager {
	/// <summary></summary>

	// ! INVISIBLE FOR USERS IN INSPECTOR !

	// Component automatically added or removed.
	// A GameObject can have multiple components of this type: one for each GameObject touched
	// which has a TriggerSensitive2D component.

	//
	// Allow to set a gameObject as a target of a collision in order to solve a Unity problem:
	//		When a gameobject was deleted as it was a target of a collision (registered in the 
	// 		source), the collision exit event is not fired in the gameobject source, so we can't
	//		unregister it of the source.
	//		This component solves the problem by implementing an OnDestroy callback (called by
	//		Unity) to unregister it when it was destroyed.
	//
	[AddComponentMenu("")]
	public class TriggerSensitive2DTarget : MonoBehaviour {
		/// <summary></summary>

		// Setted in the source TriggerSensitive2D.OnTriggerEnter.
		public TriggerSensitive2D _source;

		// Hides the component in inspector.
		private void Awake() {
			this.hideFlags = HideFlags.HideInInspector;
		}

		// Unsubscribes the target (GameObject relative to this component) from the source if necessary.
		private void OnDestroy(){
			if(_source == null) {
				return;
			}

			_source._components.Remove(this.gameObject);

			// Remove Triggered2D component of the source gameobject if necessary.
			if(_source._components.Count != 0) {
				return;
			}

			Transform[] parents = _source.GetComponentsInParent<Transform>(true); // _source.transform is include

			// If there is a DestroyGameObject action on the source gameobject or on its parents, nothing to do.
			foreach(IGameObjectManagerAction action in GameObjectManager._delayedActions) {
				if(action.GetType() == typeof(DestroyGameObject)) {
					GameObject go = (action as DestroyGameObject)._gameObject;
					foreach(Transform t in parents) {
						if(t.gameObject == go) {
							return;
						}
					}
				}
			}

			// This action will be added and treated in the current preprocess operation.
			// See MainLoop.preprocess for details.
			GameObjectManager.removeComponent<Triggered2D>(_source.gameObject);
			_source._inCollision = false;
		}
	}
}