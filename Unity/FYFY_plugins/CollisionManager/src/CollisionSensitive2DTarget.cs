using UnityEngine;
using FYFY;

namespace FYFY_plugins.CollisionManager {
	[AddComponentMenu("")]
	//[HideInInspector]
	public class CollisionSensitive2DTarget : MonoBehaviour { // one by collision so multiple possible
		public CollisionSensitive2D _source;

		private void OnDestroy(){
			if(_source == null || _source._destroying == true) {
				return;
			}
			
			_source._collisions.Remove(this.gameObject);
			_source._components.Remove(this.gameObject);

			if(_source._collisions.Count == 0) {
				foreach(FYFY.IGameObjectManagerAction action in FYFY.GameObjectManager._delayedActions) {
					if(action.GetType() == typeof(DestroyGameObject)) {
						if(((DestroyGameObject)action)._gameObject == _source.gameObject) {
							return;
						}
					}
				}

				GameObjectManager.removeComponent<InCollision2D>(_source.gameObject);
				_source._inCollision = false;
			}
		}
	}
}