using UnityEngine;
using System.Collections.Generic;
using FYFY;

namespace FYFY_plugins.TriggerManager {
	/// <summary>
	/// 	Component allowing GameObject to be noticed when it is in contact with another GameObject.
	/// </summary>
	[DisallowMultipleComponent]
	public class TriggerSensitive2D : MonoBehaviour {
		// Target / TriggerSensitive2DTarget corresponding (ie link between this gameobject and the target)
		internal Dictionary<GameObject, TriggerSensitive2DTarget> _components = new Dictionary<GameObject, TriggerSensitive2DTarget>();

		internal bool _inCollision = false;

		private void OnTriggerEnter2D(Collider2D other){
			GameObject target = other.gameObject;
			if (_components.ContainsKey(target))
				return;
			// We don't want that FYFY treates TriggerSensitive2DTarget component.
			TriggerSensitive2DTarget tst = target.gameObject.AddComponent<TriggerSensitive2DTarget>();
			tst._source = this;

			_components.Add(target, tst);

			if(_inCollision == false) {
				// This action will be treated immediatly after all the collisions
				// (next preprocess operation is done before the next simulation step).
				GameObjectManager.addComponent<Triggered2D>(this.gameObject);
				_inCollision = true;
			}
		}

		// Not fired when this GameObject or the target is destroyed.
		private void OnTriggerExit2D(Collider2D other){
			GameObject target = other.gameObject;
			TriggerSensitive2DTarget tst = _components[target];

			// Effects in TriggerSensitive2DTarget.OnDestroy
			Object.Destroy(tst);
		}

		private void OnDestroy() {
			foreach(TriggerSensitive2DTarget tst in _components.Values) {
				Object.Destroy(tst);
			}

			// Remove Triggered2D component if necessary.
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
			GameObjectManager.removeComponent<Triggered2D>(this.gameObject);
		}
	}
}