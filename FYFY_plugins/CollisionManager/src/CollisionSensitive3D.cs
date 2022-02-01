using UnityEngine;
using System.Collections.Generic;
using FYFY;
using System.Linq;

namespace FYFY_plugins.CollisionManager {
	/// <summary>
	/// 	Component allowing <c>GameObject</c> to be noticed when it is in collision with another <c>GameObject</c>.
	///     In this case the <see cref="FYFY_plugins.CollisionManager.InCollision3D"/> component (dynamically added) provides the list of <c>GameObject</c> in collision with him.
	/// </summary>
	[DisallowMultipleComponent]
	public class CollisionSensitive3D : MonoBehaviour {
		// Target / Collision informations
		private Dictionary<GameObject, Collision> _collisions = new Dictionary<GameObject, Collision>();
		// Target / CollisionSensitive3DTarget corresponding (ie link between this gameobject and the target)
		private Dictionary<GameObject, CollisionSensitive3DTarget> _targets = new Dictionary<GameObject, CollisionSensitive3DTarget>();

		private bool _inCollision = false;
		
		private bool _needUpdateCollider = false;

		private void OnCollisionEnter(Collision coll) {
			if (this.isActiveAndEnabled){
				// We check if there is an UnbindGameObject action on my gameobject or on my parents.
				// If not, we have to use FYFY to add InCollision3D in order to keep families synchronized.
				// If so, we don't add this action because it will be queued after unbind and will not be able to proceed (unknown game object).
				Transform[] parents = this.gameObject.GetComponentsInParent<Transform>(true); // this.gameobject.transform is include
				if (!GameObjectManager.containActionFor(typeof(UnbindGameObject), parents)){
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
						GameObjectManager.addComponent<InCollision3D>(this.gameObject, true);
						_inCollision = true;
					}
				}
			}
		}

		private void OnCollisionStay(Collision coll) {
			if (this.isActiveAndEnabled){
				GameObject target = coll.gameObject;
				if (_targets.ContainsKey(target))
					_collisions[target] = coll;
			}
		}

		// Not fired when this GameObject or the target is destroyed.
		private void OnCollisionExit(Collision coll) {
			if (this.isActiveAndEnabled){
				GameObject target = coll.gameObject;
				if (!_targets.ContainsKey(target))
					return;
				CollisionSensitive3DTarget cst = _targets[target];

				// Effects in CollisionSensitive3DTarget.OnDestroy
				Object.Destroy(cst);
				
				_collisions.Remove(target);
				_targets.Remove(target);
				removeInCollision();
			}
		}
		
		// Manage missing required components (Collider and RigidBody). 
		private void Update(){
			Collider coll = this.gameObject.GetComponent<Collider>();
			// If no Collider in this => we reset CollisionSensitiveTargets
			if (coll == null || !coll.enabled)
				resetCollision();
			else{
				// Parse all targets and check if they contain a Collider
				foreach (KeyValuePair<GameObject, CollisionSensitive3DTarget> entry in _targets) {
					Collider coll_target = entry.Key.GetComponent<Collider>();
					// If no Collider on target => we remove its CollisionSensitiveTarget
					if (coll_target == null || !coll_target.enabled)
						Object.Destroy(entry.Value);
					// If no RigidBody attached both Colliders => we remove this CollisionSensitiveTarget
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
			// In case of this component is inactive and GameObject is still active we will have to force update collider to process again when this component will be re-enabled
			if (!this.isActiveAndEnabled && this.gameObject.activeSelf)
				_needUpdateCollider = true;
		}
		
		private void OnEnable(){
			if (_needUpdateCollider){
				// Disable and Enable Collider to force OnCollisionEnter events
				Collider coll = GetComponent<Collider> ();
				if (coll != null && coll.enabled) {
					coll.enabled = false;
					coll.enabled = true;
				}
				_needUpdateCollider = false;
			}
		}
		
		private void resetCollision(){
			// Ask to destroy CollisionSensitive3DTarget component for each target
			foreach(CollisionSensitive3DTarget cst in _targets.Values) {
				Object.Destroy(cst);
			}
			
			removeInCollision();
		}
		
		private void removeInCollision(){
			// Check if at least one target is always defined, if not we have to remove InCollision3D component.
			if(_targets.Count == 0 && _inCollision){
				_inCollision = false;
				// We check if there is an UnbindGameObject action on my gameobject or on my parents.
				// If so, we can't use FYFY because "remove" action will be queued after unbind and will not be able to proceed (unknown game object). Then we have to remove InCollision3D component thanks to classic Unity function.
				// If not, we have to use FYFY to remove InCollision3D in order to keep families synchronized.
				Transform[] parents = this.gameObject.GetComponentsInParent<Transform>(true); // this.gameobject.transform is include
				if (GameObjectManager.containActionFor(typeof(UnbindGameObject), parents)){
					// We find an unbind action, then we remove InCollision3D component with classic Unity function
					InCollision3D component = GetComponent<InCollision3D>();
					Object.Destroy(component);
				} else {
					// We don't find an unbind action then we remove InCollision3D component with FYFY
					// This action will be added and treated in the current preprocess operation.
					// See MainLoop.preprocess for details.
					GameObjectManager.removeComponent<InCollision3D>(this.gameObject, true);
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
		
		internal Collision[] getCollisions() {
			return _collisions.Values.ToArray();
		}
	}
}