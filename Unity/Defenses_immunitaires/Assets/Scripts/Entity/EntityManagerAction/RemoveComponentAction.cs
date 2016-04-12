using UnityEngine;

internal class RemoveComponentAction<T> : IEntityManagerAction where T : Component {
	private readonly int _gameObjectId;
	private readonly uint _componentTypeId;

	internal RemoveComponentAction(GameObject gameObject) {
		Component component = gameObject.GetComponent<T>();

		if (component == null)
			throw new MissingComponentException();

		_gameObjectId = gameObject.GetInstanceID();
		_componentTypeId = TypeManager.getTypeId(typeof(T));

		Object.Destroy(component);
		Debug.Log ("COMPONENT REMOVED : " + typeof(T));
	}

	void IEntityManagerAction.perform() {
		UECS.EntityWrapper entityWrapper = UECS.EntityManager._entityWrappers[_gameObjectId];
		entityWrapper._componentTypeIds.Remove(_componentTypeId);

		FamilyManager.updateAfterComponentsUpdated(_gameObjectId, entityWrapper);
	}
}