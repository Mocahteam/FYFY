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
			
			Object.DestroyImmediate(_gameObject);
			GameObjectManager._gameObjectWrappers.Remove(gameObjectId);
			GameObjectManager._destroyedGameObjectIds.Add(gameObjectId);
		}
	}
}