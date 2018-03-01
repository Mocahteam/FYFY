using UnityEngine;
using System.Collections.Generic;
using FYFY;
using System.Linq;

namespace FYFY_plugins.TriggerManager {
	/// <summary>
	/// 	TriggerSensitive2D allows GameObject to be noticed when it is in contact with another GameObject.
	///     In this case the Triggered2d component (dynamically added) provides the list of GameObject in contact with him.
	/// </summary>
	[DisallowMultipleComponent]
	public class TriggerSensitive2D : MonoBehaviour {
		// Target / TriggerSensitive2DTarget corresponding (ie link between this gameobject and the target)
		private Dictionary<GameObject, TriggerSensitive2DTarget> _targets = new Dictionary<GameObject, TriggerSensitive2DTarget>();

		private bool _inCollision = false;
		
		private bool _needUpdateCollider = false;

		private void OnTriggerEnter2D(Collider2D other){
			if (this.isActiveAndEnabled){
				// We check if there is an UnbindGameObject action on my gameobject or on my parents.
				// If not, we have to use FYFY to add Triggered2D in order to keep families synchronized.
				// If so, we don't add this action because it will be queued after unbind and will not be able to proceed (unknown game object).
				Transform[] parents = this.gameObject.GetComponentsInParent<Transform>(true); // this.gameobject.transform is include
				if (!GameObjectManager.containUnbindActionFor(parents)){
					GameObject target = other.gameObject;
					if (_targets.ContainsKey(target))
						return;
					// We don't want that FYFY treates TriggerSensitive2DTarget component so we add component with classic Unity function.
					TriggerSensitive2DTarget tst = target.gameObject.AddComponent<TriggerSensitive2DTarget>();
					tst._source = this;

					_targets.Add(target, tst);

					if(_inCollision == false) {
						// This action will be treated after all the collisions
						// (next preprocess operation is done before the next simulation step).
						GameObjectManager.addComponent<Triggered2D>(this.gameObject, true);
						_inCollision = true;
					}
				}
			}
		}

		// Not fired when required components (Collider2D or RigidBody2D) are destroyed or disabled into this GameObject or targets.
		private void OnTriggerExit2D(Collider2D other){
			if (this.isActiveAndEnabled){
				GameObject target = other.gameObject;
				if (!_targets.ContainsKey(target))
					return;
				TriggerSensitive2DTarget tst = _targets[target];

				// Effects in TriggerSensitive2DTarget.OnDestroy
				Object.Destroy(tst);
				
				_targets.Remove(target);
				
				removeTriggered();
			}
		}
		
		// Manage missing required components (Collider2D and RigidBody2D). 
		private void Update(){
			Collider2D coll = this.gameObject.GetComponent<Collider2D>();
			// If no Collider2D in this => we reset TriggerSensitive2DTargets
			if (coll == null || !coll.enabled)
				resetTriggered();
			else{
				// Parse all targets and check if they contain a Collider2D
				foreach (KeyValuePair<GameObject, TriggerSensitive2DTarget> entry in _targets) {
					Collider2D coll_target = entry.Key.GetComponent<Collider2D>();
					// If no Collider2D on target => we remove its TriggerSensitive2DTarget
					if (coll_target == null || !coll_target.enabled)
						Object.Destroy(entry.Value);
					// If no RigidBody2D attached both Collider2Ds => we remove this TriggerSensitive2DTarget
					else if (coll.attachedRigidbody == null && coll_target.attachedRigidbody == null)
						Object.Destroy(entry.Value);
				}
				removeTriggered();
			}
		}

		private void OnDestroy() {
			resetTriggered();
		}
		
		private void OnDisable() {
			resetTriggered();
			// In case of this component is inactive and GameObject is still active we will have to force update collider2D to process again when this component will be re-enabled
			if (!this.isActiveAndEnabled && this.gameObject.activeSelf)
				_needUpdateCollider = true;
		}
		
		private void OnEnable(){
			if (_needUpdateCollider){
				// Disable and Enable Collider2D to force OnTriggerEnter2D events
				Collider2D coll = GetComponent<Collider2D> ();
				if (coll != null && coll.enabled) {
					coll.enabled = false;
					coll.enabled = true;
				}
				_needUpdateCollider = false;
			}
		}
		
		private void resetTriggered(){
			// Ask to destroy TriggerSensitive2DTarget component for each target
			foreach(TriggerSensitive2DTarget tst in _targets.Values) {
				Object.Destroy(tst);
			}
			
			removeTriggered();
		}
		
		private void removeTriggered(){
			// Check if at least one target is always defined, if not we have to remove Triggered2D component.
			if(_targets.Count == 0 && _inCollision){
				_inCollision = false;
				// We check if there is an UnbindGameObject action on my gameobject or on my parents.
				// If not, we have to use FYFY to remove Triggered2D in order to keep families synchronized.
				// If so, we can't use FYFY because "remove" action will be queued after unbind and will not be able to proceed (unknown game object). Then we have to remove Triggered2D component thanks to classic Unity function.
				Transform[] parents = this.gameObject.GetComponentsInParent<Transform>(true); // this.gameobject.transform is include
				if (GameObjectManager.containUnbindActionFor(parents)){
					// We find an unbind action, then we remove Triggered2D component with classic Unity function
					Triggered2D component = GetComponent<Triggered2D>();
					Object.Destroy(component);
				} else {
					// We don't find an unbind action then we remove Triggered2D component with FYFY
					// This action will be added and treated in the current preprocess operation.
					// See MainLoop.preprocess for details.
					GameObjectManager.removeComponent<Triggered2D>(this.gameObject, true);
				}
			}
		}
		
		// Unregister a target
		internal void unregisterTarget(GameObject target){
			_targets.Remove(target); // remove from dictionary the link with the target
			removeTriggered();	
		}
		
		internal GameObject[] getTargets() {
			return _targets.Keys.ToArray();
		}
	}
}