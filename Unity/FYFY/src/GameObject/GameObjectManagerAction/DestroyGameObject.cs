using UnityEngine;

namespace FYFY {
	internal class DestroyGameObject : IGameObjectManagerAction {
		private readonly GameObject _gameObject;

		internal DestroyGameObject(GameObject gameObject) {
			if (gameObject == null)
				throw new MissingReferenceException();
			
			_gameObject = gameObject;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null)
				throw new MissingReferenceException();
			
			int gameObjectId = _gameObject.GetInstanceID();
			if(GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId) == false)
				throw new UnityException(); // own exception

			GameObjectManager._gameObjectWrappers.Remove(gameObjectId);
			GameObjectManager._destroyedGameObjectIds.Add(gameObjectId);

			foreach(Transform transform in _gameObject.GetComponentsInChildren<Transform>(true)) { // GERER LES ENFANTS CAR ILS VONT AUSSI ETRE DETRUITS !
				int childId = transform.gameObject.GetInstanceID();

				if(GameObjectManager._gameObjectWrappers.ContainsKey(childId) == false)
					throw new UnityException(); // own exception

				GameObjectManager._gameObjectWrappers.Remove(childId);
				GameObjectManager._destroyedGameObjectIds.Add(childId);
			}

			Object.DestroyImmediate(_gameObject);
		}
	}
}