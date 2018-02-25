using UnityEngine;
using System.Collections.Generic;
using FYFY;
using System.Linq;

namespace FYFY_plugins.TriggerManager {
	/// <summary>
	/// 	TriggerSensitive3D allows GameObject to be noticed when it is in contact with another GameObject.
	///     In this case the Triggered3d component (dynamically added) provides the list of GameObject in contact with him.
	/// </summary>
	[DisallowMultipleComponent]
	public class TriggerSensitive3D : MonoBehaviour {
		// Target / TriggerSensitive3DTarget corresponding (ie link between this gameobject and the target)
		private Dictionary<GameObject, TriggerSensitive3DTarget> _targets = new Dictionary<GameObject, TriggerSensitive3DTarget>();

		private bool _inCollision = false;
		
		private bool _needUpdateCollider = false;

		private void OnTriggerEnter(Collider other){
			if (this.isActiveAndEnabled){
				GameObject target = other.gameObject;
				if (_targets.ContainsKey(target))
					return;
				// We don't want that FYFY treates TriggerSensitive3DTarget component so we add component with classic Unity function.
				TriggerSensitive3DTarget tst = target.gameObject.AddComponent<TriggerSensitive3DTarget>();
				tst._source = this;

				_targets.Add(target, tst);

				if(_inCollision == false) {
					// This action will be treated after all the collisions
					// (next preprocess operation is done before the next simulation step).
					GameObjectManager.addComponent<Triggered3D>(this.gameObject);
					_inCollision = true;
				}
			}
		}

		// Not fired when required components (Colliders or RigidBody) are destroyed or disabled into this GameObject or targets.
		private void OnTriggerExit(Collider other){
			if (this.isActiveAndEnabled){
				GameObject target = other.gameObject;
				if (!_targets.ContainsKey(target))
					return;
				TriggerSensitive3DTarget tst = _targets[target];

				// Effects in TriggerSensitive3DTarget.OnDestroy
				Object.Destroy(tst);
				
				_targets.Remove(target);
				
				manageTriggered();
			}
		}
		
		// Manage missing required components (Collider and RigidBody). 
		private void Update(){
			Collider coll = this.gameObject.GetComponent<Collider>();
			// If no Collider in this => we reset TriggerSensitive3DTargets
			if (coll == null || !coll.enabled)
				resetTriggered();
			else{
				// Parse all targets and check if they contain a Collider
				foreach (KeyValuePair<GameObject, TriggerSensitive3DTarget> entry in _targets) {
					Collider coll_target = entry.Key.GetComponent<Collider>();
					// If no Collider on target => we remove its TriggerSensitive3DTarget
					if (coll_target == null || !coll_target.enabled)
						Object.Destroy(entry.Value);
					// If no RigidBody attached both Colliders => we remove this TriggerSensitive3DTarget
					else if (coll.attachedRigidbody == null && coll_target.attachedRigidbody == null)
						Object.Destroy(entry.Value);
				}
			}
			manageTriggered();
		}

		private void OnDestroy() {
			resetTriggered();
		}
		
		private void OnDisable() {
			resetTriggered();
			// In case of this component is inactive and GameObject is still active we will have to force update collider to process again when this component will be re-enabled
			if (!this.isActiveAndEnabled && this.gameObject.activeSelf)
				_needUpdateCollider = true;
		}
		
		private void OnEnable(){
			if (_needUpdateCollider){
				// Disable and Enable Collider to force OnTriggerEnter events
				Collider coll = GetComponent<Collider> ();
				if (coll != null && coll.enabled) {
					coll.enabled = false;
					coll.enabled = true;
				}
				_needUpdateCollider = false;
			}
		}
		
		private void resetTriggered (){
			// Ask to destroy TriggerSensitive3DTarget component for each target
			foreach(TriggerSensitive3DTarget tst in _targets.Values) {
				Object.Destroy(tst);
			}
			
			manageTriggered();
		}
		
		private void manageTriggered(){
			// Check if at least one target is always defined, if not we have to remove Triggered3D component.
			if(_targets.Count == 0 && _inCollision){
				_inCollision = false;
				Transform[] parents = this.gameObject.GetComponentsInParent<Transform>(true); // this.gameobject.transform is include

				// We check if there is an UnbindGameObject action on my gameobject or on my parents.
				// If not, we have to use FYFY to remove Triggered3D in order to keep families synchronized.
				// If so, we can't use FYFY because "remove" action will be queued after unbind and will not be able to proceed (unknown game object). Then we have to remove Triggered3D component thanks to classic Unity function.
				foreach(IGameObjectManagerAction action in GameObjectManager._delayedActions) {
					if(action.GetType() == typeof(UnbindGameObject)) {
						GameObject go = (action as UnbindGameObject)._gameObject;
						foreach(Transform t in parents) {
							if(t.gameObject == go) {
								// We find an unbind action, then we remove Triggered3D component with classic Unity function
								Triggered3D component = GetComponent<Triggered3D>();
								Object.Destroy(component);
								return;
							}
						}
					}
				}
				// We don't find an unbind action then we remove Triggered3D component with FYFY
				// This action will be added and treated in the current preprocess operation.
				// See MainLoop.preprocess for details.
				GameObjectManager.removeComponent<Triggered3D>(this.gameObject);
			}
		}
		
		// Unregister a target
		internal void unregisterTarget(GameObject target){
			_targets.Remove(target); // remove from dictionary the link with the target
			manageTriggered();
		}
		
		internal GameObject[] getTargets() {
			return _targets.Keys.ToArray();
		}
	}
}