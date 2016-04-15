using UnityEngine;

internal class RemoveEntityAction : IEntityManagerAction {
	private readonly int _entityWrapperId;

	internal RemoveEntityAction(GameObject gameObject) {
		_entityWrapperId = gameObject.GetInstanceID();

		Object.Destroy(gameObject);
	}

	void IEntityManagerAction.perform() {
		UECS.EntityManager._entityWrappers.Remove(_entityWrapperId);
		FamilyManager.updateAfterEntityRemoved(_entityWrapperId);
	}
}
