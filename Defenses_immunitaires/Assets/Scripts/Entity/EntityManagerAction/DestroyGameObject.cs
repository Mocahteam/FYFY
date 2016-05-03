using UnityEngine;

internal class DestroyGameObject : IEntityManagerAction {
	private readonly GameObject _gameObject;

	internal DestroyGameObject(GameObject gameObject) {
		_gameObject = gameObject;
	}

	void IEntityManagerAction.perform() {
		if(_gameObject == null)
			throw new MissingComponentException();
		
		int gameObjectId = _gameObject.GetInstanceID();
		if(EntityManager._gameObjectWrappers.ContainsKey(gameObjectId) == false)
			throw new UnityException(); // own exception
		
		Object.DestroyImmediate(_gameObject);
		EntityManager._gameObjectWrappers.Remove(gameObjectId);
		EntityManager._destroyedGameObjectIds.Add(gameObjectId);
	}
}