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
		
		private bool _needUpdateCollider = false;

		private void OnCollisionEnter2D(Collision2D coll) {
			if (this.isActiveAndEnabled){
				// We check if there is an UnbindGameObject action on my gameobject or on my parents.
				// If not, we have to use FYFY to add InCollision2D in order to keep families synchronized.
				// If so, we don't add this action because it will be queued after unbind and will not be able to proceed (unknown game object).
				Transform[] parents = this.gameObject.GetComponentsInParent<Transform>(true); // this.gameobject.transform is include
				if (!GameObjectManager.containUnbindActionFor(parents)){
					// We don't find an unbind action, then we can add PointerOver component with classic Unity function
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
						GameObjectManager.addComponent<InCollision2D>(this.gameObject, true);
						_inCollision = true;
					}
				}
			}
		}

		private void OnCollisionStay2D(Collision2D coll) {
			if (this.isActiveAndEnabled){
				GameObject target = coll.gameObject;
				if (_targets.ContainsKey(target))
					_collisions[target] = coll;
			}
		}

		// Not fired when required components (Collider2D or RigidBody2D) are destroyed or disabled into this GameObject or targets.
		private void OnCollisionExit2D(Collision2D coll) {
			if (this.isActiveAndEnabled){
				GameObject target = coll.gameObject;
				if (!_targets.ContainsKey(target))
					return;
				CollisionSensitive2DTarget cst = _targets[target];

				// Effects in CollisionSensitive2DTarget.OnDestroy
				Object.Destroy(cst);
				
				_collisions.Remove(target);
				_targets.Remove(target);
				removeInCollision();
			}
		}
		
		// Manage missing required components (Collider2D and RigidBody2D). 
		private void Update(){
			Collider2D coll = this.gameObject.GetComponent<Collider2D>();
			// If no Collider2D in this => we reset CollisionSensitive2DTargets
			if (coll == null || !coll.enabled)
				resetCollision();
			else{
				// Parse all targets and check if they contain a Collider2D
				foreach (KeyValuePair<GameObject, CollisionSensitive2DTarget> entry in _targets) {
					Collider2D coll_target = entry.Key.GetComponent<Collider2D>();
					// If no Collider2D on target => we remove its CollisionSensitive2DTarget
					if (coll_target == null || !coll_target.enabled)
						Object.Destroy(entry.Value);
					// If no RigidBody2D attached both Collider2Ds => we remove this CollisionSensitive2DTarget
					else if (coll.attachedRigidbody == null && coll_target.attachedRigidbody == null)
						Object.Destroy(entry.Value);
				}
				removeInCollision();
			}
		}

		private void OnDestroy() {
			resetCollision();
		}
		
		private void OnDisable() {
			resetCollision();
			// In case of this component is inactive and GameObject is still active we will have to force update collider2D to process again when this component will be re-enabled
			if (!this.isActiveAndEnabled && this.gameObject.activeSelf)
				_needUpdateCollider = true;
		}
		
		private void OnEnable(){
			if (_needUpdateCollider){
				// Disable and Enable Collider2D to force OnCollisionEnter2D events
				Collider2D coll = GetComponent<Collider2D> ();
				if (coll != null && coll.enabled) {
					coll.enabled = false;
					coll.enabled = true;
				}
				_needUpdateCollider = false;
			}
		}
		
		private void resetCollision(){
			// Ask to destroy CollisionSensitive2DTarget component for each target
			foreach(CollisionSensitive2DTarget cst in _targets.Values) {
				Object.Destroy(cst);
			}
			
			removeInCollision();
		}
		
		private void removeInCollision(){
			// Check if at least one target is always defined, if not we have to remove InCollision2D component.
			if(_targets.Count == 0 && _inCollision){
				_inCollision = false;
				// We check if there is an UnbindGameObject action on my gameobject or on my parents.
				// If so, we can't use FYFY because "remove" action will be queued after unbind and will not be able to proceed (unknown game object). Then we have to remove InCollision2D component thanks to classic Unity function.
				// If not, we have to use FYFY to remove InCollision2D in order to keep families synchronized.
				Transform[] parents = this.gameObject.GetComponentsInParent<Transform>(true); // this.gameobject.transform is include
				if (GameObjectManager.containUnbindActionFor(parents)){
					// We find an unbind action, then we remove InCollision2D component with classic Unity function
					InCollision2D component = GetComponent<InCollision2D>();
					Object.Destroy(component);
				} else {
					// We don't find an unbind action then we remove InCollision2D component with FYFY
					// This action will be added and treated in the current preprocess operation.
					// See MainLoop.preprocess for details.
					GameObjectManager.removeComponent<InCollision2D>(this.gameObject, true);
				}
			}
		}
		
		// Unregister a target
		internal void unregisterTarget(GameObject target){
			// remove from dictionary the links with the target
			_collisions.Remove(target);
			_targets.Remove(target);
			removeInCollision();
		}
		
		internal GameObject[] getTargets() {
			return _targets.Keys.ToArray();
		}
		
		internal Collision2D[] getCollisions() {
			return _collisions.Values.ToArray();
		}
	}
}