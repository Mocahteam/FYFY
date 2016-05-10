using UnityEngine;

internal class EnableGameObject : IGameObjectManagerAction {
	private readonly GameObject _gameObject;

	internal EnableGameObject(GameObject gameObject) {
		if (gameObject == null)
			throw new MissingReferenceException();

		_gameObject = gameObject;
	}

	void IGameObjectManagerAction.perform() {
		if(_gameObject == null)
			throw new MissingReferenceException();

		if(_gameObject.activeSelf == false) {
			_gameObject.SetActive(true);
			GameObjectManager._modifiedGameObjectIds.Add(_gameObject.GetInstanceID());
		}
	}
}