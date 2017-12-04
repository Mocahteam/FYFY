using UnityEngine;
using System.Collections.Generic;
using FYFY;
using System.Linq;

namespace FYFY_plugins.CollisionManager {
	/// <summary>
	/// 	CollisionSensitive3D allows GameObject to be noticed when it is in contact with another GameObject.
	///     In this case the InCollision3d component (dynamically added) provides the list of GameObject in contact with him.
	/// </summary>
	[DisallowMultipleComponent]
	public class CollisionSensitive3D : MonoBehaviour {
		// Target / Collision informations
		private Dictionary<GameObject, Collision> _collisions = new Dictionary<GameObject, Collision>();
		// Target / CollisionSensitive3DTarget corresponding (ie link between this gameobject and the target)
		private Dictionary<GameObject, CollisionSensitive3DTarget> _targets = new Dictionary<GameObject, CollisionSensitive3DTarget>();

		private bool _inCollision = false;

		private void OnCollisionEnter(Collision coll) {
			GameObject target = coll.gameObject;
			if (_targets.ContainsKey(target))
				return;
			// We don't want that FYFY treates CollisionSensitive3DTarget component so we add component with classic Unity function.
			CollisionSensitive3DTarget cst = target.gameObject.AddComponent<CollisionSensitive3DTarget>();
			cst._source = this;

			_collisions.Add(target, coll);
			_targets.Add(target, cst);

			if(_inCollision == false) {
				// This action will be treated immediatly after all the collisions
				// (next preprocess operation is done before the next simulation step).
				GameObjectManager.addComponent<InCollision3D>(this.gameObject);
				_inCollision = true;
			}
		}

		private void OnCollisionStay(Collision coll) {
			GameObject target = coll.gameObject;
			if (_targets.ContainsKey(target))
				_collisions[target] = coll;
		}

		// Not fired when this GameObject or the target is destroyed.
		private void OnCollisionExit(Collision coll) {
			GameObject target = coll.gameObject;
			if (!_targets.ContainsKey(target))
				return;
			CollisionSensitive3DTarget cst = _targets[target];

			// Effects in CollisionSensitive3DTarget.OnDestroy
			Object.Destroy(cst);
			
			_collisions.Remove(target);
			_targets.Remove(target);
			manageInCollision();
		}

		private void OnDestroy() {
			// Ask to destroy CollisionSensitive3DTarget component for each target
			foreach(CollisionSensitive3DTarget cst in _targets.Values) {
				Object.Destroy(cst);
			}
			
			if (_inCollision){
				InCollision3D component = GetComponent<InCollision3D>();
				Object.Destroy(component);
			}
		}
		
		private void manageInCollision(){
			// Check if at least one target is always defined, if not we have to remove InCollision3D component.
			if(_targets.Count == 0 && _inCollision){
				_inCollision = false;
				Transform[] parents = this.gameObject.GetComponentsInParent<Transform>(true); // this.gameobject.transform is include

				// We check if there is an UnbindGameObject action on my gameobject or on my parents.
				// If so, we can't use FYFY because "remove" action will be queued after unbind and will not be able to proceed (unknown game object). Then we have to remove InCollision3D component thanks to classic Unity function.
				// If not, we have to use FYFY to remove InCollision3D in order to keep families synchronized.
				foreach(IGameObjectManagerAction action in GameObjectManager._delayedActions) {
					if(action.GetType() == typeof(UnbindGameObject)) {
						GameObject go = (action as UnbindGameObject)._gameObject;
						foreach(Transform t in parents) {
							if(t.gameObject == go) {
								// We find an unbind action, then we remove InCollision3D component with classic Unity function
								InCollision3D component = GetComponent<InCollision3D>();
								Object.Destroy(component);
								return;
							}
						}
					}
				}
				// We don't find an unbind action then we remove InCollision3D component with FYFY
				// This action will be added and treated in the current preprocess operation.
				// See MainLoop.preprocess for details.
				GameObjectManager.removeComponent<InCollision3D>(this.gameObject);
			}
		}
		
		// Unregister a target
		internal void unregisterTarget(GameObject target){
			// remove from dictionary the links with the target
			_collisions.Remove(target);
			_targets.Remove(target);
			manageInCollision();
		}
		
		internal GameObject[] getTargets() {
			return _targets.Keys.ToArray();
		}
		
		internal Collision[] getCollisions() {
			return _collisions.Values.ToArray();
		}
	}
}