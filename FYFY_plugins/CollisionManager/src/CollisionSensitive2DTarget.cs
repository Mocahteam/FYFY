using UnityEngine;
using FYFY;

namespace FYFY_plugins.CollisionManager {
	/// <summary></summary>

	// One by collision so multiple possible

	[AddComponentMenu("")]
	public class CollisionSensitive2DTarget : MonoBehaviour {
		/// <summary></summary>

		// Setted in the source CollisionSensitive2D.OnCollisionEnter.
		public CollisionSensitive2D _source;

		// Hides the component in inspector.
		private void Awake() {
			this.hideFlags = HideFlags.HideInInspector;
		}

		// Unsubscribes the target (GameObject relatives to this component) from the source if necessary.
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

				//
				// Two important origins of this OnDestroy calling:
				// 		- The source CollisionSensitive2D.OnCollisionExit2D,
				// 		- GameObjectManager.Destroy with the relative GameObject to this component.
				// ->
				// ... ???
				//
				GameObjectManager.removeComponent<InCollision2D>(_source.gameObject);
				_source._inCollision = false;
			}
		}
	}
}