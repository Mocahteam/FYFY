﻿using UnityEngine;
using FYFY;

namespace FYFY_plugins.CollisionManager {
	/// <summary></summary>

	// ! INVISIBLE FOR USERS IN INSPECTOR !

	// Component automatically added or removed.
	// A GameObject can have multiple components of this type: one for each GameObject touched
	// which has a CollisionSensitive2D component.

	//
	// Allow to set a gameObject as a target of a collision in order to solve a Unity problem:
	//		When a gameobject was deleted as it was a target of a collision (registered in the 
	// 		source), the collision exit event is not fired in the gameobject source, so we can't
	//		unregister it of the source.
	//		This component solves the problem by implementing an OnDestroy callback (called by
	//		Unity) to unregister it when it was destroyed.
	//
	[AddComponentMenu("")]
	public class CollisionSensitive2DTarget : MonoBehaviour {
		/// <summary></summary>

		// Setted in the source CollisionSensitive2D.OnCollisionEnter.
		public CollisionSensitive2D _source;

		// Hides the component in inspector.
		private void Awake() {
			this.hideFlags = HideFlags.HideInInspector;
		}

		// Unsubscribes the target (GameObject relative to this component) from the source if necessary.
		private void OnDestroy(){
			if(_source == null) {
				return;
			}
			
			_source._collisions.Remove(this.gameObject);
			_source._components.Remove(this.gameObject);

			// Remove source InCollision2D component if necessary.
			if(_source._collisions.Count == 0) {
				// If there is a DestroyGameObject action on the source, nothing to do.
				foreach(FYFY.IGameObjectManagerAction action in FYFY.GameObjectManager._delayedActions) {
					if(action.GetType() == typeof(DestroyGameObject)) {
						if(((DestroyGameObject)action)._gameObject == _source.gameObject) {
							return;
						}
					}
				}

				// This action will be added and treated in the current preprocess operation.
				// See MainLoop.preprocess for details.
				GameObjectManager.removeComponent<InCollision2D>(_source.gameObject);
				_source._inCollision = false;
			}
		}
	}
}