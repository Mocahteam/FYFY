using UnityEngine;
using System.Collections.Generic;
using FYFY;

namespace FYFY_plugins.TriggerManager {
	/// <summary>
	/// 	Component allowing GameObject to be noticed when it is in contact with another GameObject.
	/// </summary>
	[DisallowMultipleComponent]
	public class TriggerSensitive3D : MonoBehaviour {
		// Target / TriggerSensitive3DTarget corresponding (ie link between this gameobject and the target)
		internal Dictionary<GameObject, TriggerSensitive3DTarget> _components = new Dictionary<GameObject, TriggerSensitive3DTarget>();

		internal bool _inCollision = false;

		private void OnTriggerEnter(Collider other){
			GameObject target = other.gameObject;
			// We don't want that FYFY treates TriggerSensitive3DTarget component.
			TriggerSensitive3DTarget tst = target.gameObject.AddComponent<TriggerSensitive3DTarget>();
			tst._source = this;

			_components.Add(target, tst);

			if(_inCollision == false) {
				// This action will be treated immediatly after all the collisions
				// (next preprocess operation is done before the next simulation step).
				GameObjectManager.addComponent<Triggered3D>(this.gameObject);
				_inCollision = true;
			}
		}

		// Not fired when this GameObject or the target is destroyed.
		private void OnTriggerExit(Collider other){
			GameObject target = other.gameObject;
			TriggerSensitive3DTarget tst = _components[target];

			// Effects in TriggerSensitive3DTarget.OnDestroy
			Object.Destroy(tst);
		}

		private void OnDestroy() {
			foreach(TriggerSensitive3DTarget tst in _components.Values) {
				Object.Destroy(tst);
			}

			// Remove Triggered3D component if necessary.
			if(_inCollision == false) {
				return;
			}

			Transform[] parents = this.gameObject.GetComponentsInParent<Transform>(true); // this.gameobject.transform is include

			// If there is a DestroyGameObject action on my gameobject or on my parents, nothing to do.
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
			GameObjectManager.removeComponent<Triggered3D>(this.gameObject);
		}
	}
}