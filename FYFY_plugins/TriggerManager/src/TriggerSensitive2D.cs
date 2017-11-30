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

		private void OnTriggerEnter2D(Collider2D other){
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
				GameObjectManager.addComponent<Triggered2D>(this.gameObject);
				_inCollision = true;
			}
		}

		// Not fired when this GameObject or the target is destroyed.
		private void OnTriggerExit2D(Collider2D other){
			GameObject target = other.gameObject;
			if (!_targets.ContainsKey(target))
				return;
			TriggerSensitive2DTarget tst = _targets[target];

			// Effects in TriggerSensitive2DTarget.OnDestroy
			Object.Destroy(tst);
			
			_targets.Remove(target);
			
			manageTriggered();
		}

		private void OnDestroy() {
			// Ask to destroy TriggerSensitive2DTarget component for each target
			foreach(TriggerSensitive2DTarget tst in _targets.Values) {
				Object.Destroy(tst);
			}
			
			if (_inCollision){
				Triggered2D component = GetComponent<Triggered2D>();
				Object.Destroy(component);
			}
		}
		
		private void manageTriggered(){
			// Check if at least one target is always defined, if not we have to remove Triggered2D component.
			if(_targets.Count == 0 && _inCollision){
				_inCollision = false;
				Transform[] parents = this.gameObject.GetComponentsInParent<Transform>(true); // this.gameobject.transform is include

				// We check if there is an UnbindGameObject action on my gameobject or on my parents.
				// If not, we have to use FYFY to remove Triggered2D in order to keep families synchronized.
				// If so, we can't use FYFY because "remove" action will be queued after unbind and will not be able to proceed (unknown game object). Then we have to remove Triggered2D component thanks to classic Unity function.
				foreach(IGameObjectManagerAction action in GameObjectManager._delayedActions) {
					if(action.GetType() == typeof(UnbindGameObject)) {
						GameObject go = (action as UnbindGameObject)._gameObject;
						foreach(Transform t in parents) {
							if(t.gameObject == go) {
								// We find an unbind action, then we remove Triggered2D component with classic Unity function
								Triggered2D component = GetComponent<Triggered2D>();
								Object.Destroy(component);
								return;
							}
						}
					}
				}
				// We don't find an unbind action then we remove Triggered2D component with FYFY
				// This action will be added and treated in the current preprocess operation.
				// See MainLoop.preprocess for details.
				GameObjectManager.removeComponent<Triggered2D>(this.gameObject);
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