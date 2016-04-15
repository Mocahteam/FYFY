using UnityEngine;

internal class RemoveComponentAction<T> : IEntityManagerAction where T : Component {
	private readonly int _entityWrapperId;
	private readonly uint _componentTypeId;

	internal RemoveComponentAction(GameObject gameObject) {
		Component component = gameObject.GetComponent<T>();

		if (component == null)
			throw new MissingComponentException();

		_entityWrapperId = gameObject.GetInstanceID();
		_componentTypeId = TypeManager.getTypeId(typeof(T));

		Object.Destroy(component);
	}

	void IEntityManagerAction.perform() {
		UECS.EntityWrapper entityWrapper = UECS.EntityManager._entityWrappers[_entityWrapperId];
		entityWrapper._componentTypeIds.Remove(_componentTypeId);

		FamilyManager.updateAfterComponentsUpdated(_entityWrapperId, entityWrapper);
	}
}