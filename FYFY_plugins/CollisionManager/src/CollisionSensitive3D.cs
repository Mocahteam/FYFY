using UnityEngine;
using System.Collections.Generic;
using FYFY;

namespace FYFY_plugins.CollisionManager {
	/// <summary>
	/// 	Component allowing GameObject to be noticed when it is in contact with another GameObject.
	/// </summary>
	[DisallowMultipleComponent]
	public class CollisionSensitive3D : MonoBehaviour {
		// Target / Collision informations
		internal Dictionary<GameObject, Collision> _collisions = new Dictionary<GameObject, Collision>();
		// Target / CollisionSensitive3DTarget corresponding (ie link between this gameobject and the target)
		internal Dictionary<GameObject, CollisionSensitive3DTarget> _components = new Dictionary<GameObject, CollisionSensitive3DTarget>();

		internal bool _inCollision = false;

		private void OnCollisionEnter(Collision coll) {
			GameObject target = coll.gameObject;
			// We don't want that FYFY treates CollisionSensitive3DTarget component.
			CollisionSensitive3DTarget cst = target.gameObject.AddComponent<CollisionSensitive3DTarget>();
			cst._source = this;

			_collisions.Add(target, coll);
			_components.Add(target, cst);

			if(_inCollision == false) {
				// This action will be treated immediatly after all the collisions
				// (next preprocess operation is done before the next simulation step).
				GameObjectManager.addComponent<InCollision3D>(this.gameObject);
				_inCollision = true;
			}
		}

		private void OnCollisionStay(Collision coll) {
			GameObject target = coll.gameObject;
			_collisions[target] = coll;
		}

		// Not fired when this GameObject or the target is destroyed.
		private void OnCollisionExit(Collision coll) {
			GameObject target = coll.gameObject;
			CollisionSensitive3DTarget cst = _components[target];

			// Effects in CollisionSensitive3DTarget.OnDestroy
			Object.Destroy(cst);
		}

		private void OnDestroy() {
			foreach(CollisionSensitive3DTarget cst in _components.Values) {
				Object.Destroy(cst);
			}

			// Remove InCollision3D component if necessary.
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
			GameObjectManager.removeComponent<InCollision3D>(this.gameObject);
		}
	}
}