using UnityEngine;
using System.Linq;

namespace FYFY {
	internal class DestroyGameObject : IGameObjectManagerAction {
		internal readonly GameObject _gameObject; // internal to be accessed in TriggerManager && CollisionManager dlls
		private  readonly string _exceptionStackTrace;

		internal DestroyGameObject(GameObject gameObject, string exceptionStrackTrace) {
			_gameObject = gameObject;
			_exceptionStackTrace = exceptionStrackTrace;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null) {
				throw new DestroyedGameObjectException(_exceptionStackTrace);
			}

			// Unregister the gameobject and all its children.
			foreach(Transform t in _gameObject.GetComponentsInChildren<Transform>(true)) { // gameobject.transform is include
				int childId = t.gameObject.GetInstanceID();

				if(GameObjectManager._gameObjectWrappers.ContainsKey(childId) == false){
					throw new UnknownGameObjectException(_exceptionStackTrace);
				}

				GameObjectManager._gameObjectWrappers.Remove(childId);
				GameObjectManager._destroyedGameObjectIds.Add(childId);
			}

			// Destroy the gameobject and all its children.
			Object.DestroyImmediate(_gameObject);
		}
	}
}