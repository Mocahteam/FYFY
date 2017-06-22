using UnityEngine;
using System.Collections.Generic;
using FYFY;
using System.Linq;

namespace FYFY_plugins.CollisionManager {
	/// <summary>
	/// 	CollisionSensitive2D allows GameObject to be noticed when it is in contact with another GameObject.
	///     In this case the InCollision2d component (dynamically added) provides the list of GameObject in contact with him.
	/// </summary>
	[DisallowMultipleComponent]
	public class CollisionSensitive2D : MonoBehaviour {
		// Target / Collision informations
		private Dictionary<GameObject, Collision2D> _collisions = new Dictionary<GameObject, Collision2D>();
		// Target / CollisionSensitive2DTarget corresponding (ie link between this gameobject and the target)
		private Dictionary<GameObject, CollisionSensitive2DTarget> _targets = new Dictionary<GameObject, CollisionSensitive2DTarget>();

		private bool _inCollision = false;

		private void OnCollisionEnter2D(Collision2D coll) {
			GameObject target = coll.gameObject;
			if (_targets.ContainsKey(target))
				return;
			// We don't want that FYFY treates CollisionSensitive2DTarget component so we add component with classic Unity function.
			CollisionSensitive2DTarget cst = target.gameObject.AddComponent<CollisionSensitive2DTarget>();
			cst._source = this;

			_collisions.Add(target, coll);
			_targets.Add(target, cst);

			if(_inCollision == false) {
				// This action will be treated after all the collisions
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
			CollisionSensitive2DTarget cst = _targets[target];

			// Effects in CollisionSensitive2DTarget.OnDestroy
			Object.Destroy(cst);
		}

		private void OnDestroy() {
			// Ask to destroy CollisionSensitive2DTarget component for each target
			foreach(CollisionSensitive2DTarget cst in _targets.Values) {
				Object.Destroy(cst);
			}
			
			if (_inCollision){
				InCollision2D component = GetComponent<InCollision2D>();
				Object.Destroy(component);
			}
		}
		
		// Unregister a target
		internal void unregisterTarget(GameObject target){
			// remove from dictionary the links with the target
			_collisions.Remove(this.gameObject);
			_targets.Remove(this.gameObject);
			
			// Check if at least one target is always defined, if not we have to remove InCollision2D component.
			if(_targets.Count == 0){
				_inCollision = false;
				Transform[] parents = this.gameObject.GetComponentsInParent<Transform>(true); // this.gameobject.transform is include

				// We check if there is an UnbindGameObject action on my gameobject or on my parents.
				// If not, we have to use FYFY to remove InCollision2D in order to keep families synchronized.
				// If so, we can't use FYFY because "remove" action will be queued after unbind and will not be able to proceed (unknown game object). Then we have to remove InCollision2D component thanks to classic Unity function.
				foreach(IGameObjectManagerAction action in GameObjectManager._delayedActions) {
					if(action.GetType() == typeof(UnbindGameObject)) {
						GameObject go = (action as UnbindGameObject)._gameObject;
						foreach(Transform t in parents) {
							if(t.gameObject == go) {
								// We find an unbind action, then we remove InCollision2D component with classic Unity function
								InCollision2D component = GetComponent<InCollision2D>();
								Object.Destroy(component);
								return;
							}
						}
					}
				}
				// We don't find an unbind action then we remove InCollision2D component with FYFY
				// This action will be added and treated in the current preprocess operation.
				// See MainLoop.preprocess for details.
				GameObjectManager.removeComponent<InCollision2D>(this.gameObject);
			}
		}
		
		internal GameObject[] getTargets() {
			return _targets.Keys.ToArray();
		}
		
		internal Collision2D[] getCollisions() {
			return _collisions.Values.ToArray();
		}
	}
}