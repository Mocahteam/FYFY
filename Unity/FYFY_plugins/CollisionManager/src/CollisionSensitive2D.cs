using UnityEngine;
using System.Collections.Generic;
using FYFY;

namespace FYFY_plugins.CollisionManager {
	/// <summary>
	/// 	Component allowing GameObject to be noticed when it is in contact with another GameObject.
	/// </summary>
	[DisallowMultipleComponent]
	public class CollisionSensitive2D : MonoBehaviour {
		// Target / Collision informations
		internal Dictionary<GameObject, Collision2D> _collisions = new Dictionary<GameObject, Collision2D>();
		// Target / CollisionSensitive2DTarget corresponding (ie link between this gameobject and the target)
		internal Dictionary<GameObject, CollisionSensitive2DTarget> _components = new Dictionary<GameObject, CollisionSensitive2DTarget>();

		internal bool _inCollision = false;

		private void OnCollisionEnter2D(Collision2D coll) {
			GameObject target = coll.gameObject;
			if (_components.ContainsKey(target))
				return;
			// We don't want that FYFY treates CollisionSensitive2DTarget component.
			CollisionSensitive2DTarget cst = target.gameObject.AddComponent<CollisionSensitive2DTarget>();
			cst._source = this;

			_collisions.Add(target, coll);
			_components.Add(target, cst);

			if(_inCollision == false) {
				// This action will be treated immediatly after all the collisions
				// (next preprocess operation is done before the next simulation step).
				GameObjectManager.addComponent<InCollision2D>(this.gameObject);
				_inCollision = true;
			}
		}

		private void OnCollisionStay2D(Collision2D coll) {
			GameObject target = coll.gameObject;
			_collisions[target] = coll;
		}

		// Not fired when this GameObject or the target is destroyed.
		private void OnCollisionExit2D(Collision2D coll) {
			GameObject target = coll.gameObject;
			CollisionSensitive2DTarget cst = _components[target];

			// Effects in CollisionSensitive2DTarget.OnDestroy
			Object.Destroy(cst);
		}

		private void OnDestroy() {
			foreach(CollisionSensitive2DTarget cst in _components.Values) {
				Object.Destroy(cst);
			}

			// Remove InCollision2D component if necessary.
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
			GameObjectManager.removeComponent<InCollision2D>(this.gameObject);
		}
	}
}