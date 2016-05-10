using UnityEngine;

internal class RemoveComponent<T> : IGameObjectManagerAction where T : Component {
	private readonly GameObject _gameObject;
	private readonly System.Type _componentType;

	internal RemoveComponent(GameObject gameObject, System.Type componentType) {
		if (gameObject == null || componentType == null)
			throw new MissingReferenceException();

		_gameObject = gameObject;
		_componentType = componentType;
	}

	void IGameObjectManagerAction.perform() {
		if (_gameObject == null || _componentType == null)
			throw new MissingReferenceException();

		int gameObjectId = _gameObject.GetInstanceID();
		if(GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId) == false)
			throw new UnityException(); // own exception

		T component = _gameObject.GetComponent<T>();
		if (component == null) {
			Debug.LogWarning("Can't remove '" + _componentType + "' from " + _gameObject.name + " because a '" + _componentType + "' is'nt attached to the game object!");
			return;
		}
		Object.DestroyImmediate(component);

		uint componentTypeId = TypeManager.getTypeId(_componentType);
		GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeIds.Remove(componentTypeId); // -> remove exception if doesnt exist
		GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
	}
}

internal class RemoveComponent : IGameObjectManagerAction {
	private readonly GameObject _gameObject;
	private readonly Component _component;
	private readonly System.Type _componentType;

	internal RemoveComponent(GameObject gameObject, Component component, System.Type componentType) {
		if (gameObject == null || component == null || componentType == null)
			throw new MissingReferenceException();

		_gameObject = gameObject;
		_component = component;
		_componentType = componentType;
	}

	void IGameObjectManagerAction.perform() {
		if (_gameObject == null || _component == null || _componentType == null)
			throw new MissingReferenceException();

		int gameObjectId = _gameObject.GetInstanceID();
		if(GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId) == false)
			throw new UnityException(); // own exception

		Object.DestroyImmediate(_component);

		uint componentTypeId = TypeManager.getTypeId(_componentType);
		GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeIds.Remove(componentTypeId); // -> remove exception if doesnt exist
		GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
	}
}