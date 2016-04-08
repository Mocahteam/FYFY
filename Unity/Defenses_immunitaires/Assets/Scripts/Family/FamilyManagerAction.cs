internal class FamilyManagerAction {
	internal enum CAUSE { COMPONENT_ADDED, COMPONENT_REMOVED, ENTITY_ADDED, ENTITY_REMOVED };

	private readonly int _entityId;
	private readonly UECS.Entity _entity;

	internal delegate void DelegatePerformMethod();
	internal readonly DelegatePerformMethod perform;

	internal FamilyManagerAction(CAUSE cause, int entityId, UECS.Entity entity) {
		_entityId = entityId;
		_entity = entity;

		switch (cause) {
		case CAUSE.COMPONENT_ADDED:
		case CAUSE.COMPONENT_REMOVED:
			perform = new DelegatePerformMethod(actionWhenComponentsUpdated);
			break;
		case CAUSE.ENTITY_ADDED:
			perform = new DelegatePerformMethod(actionWhenEntityAdded);
			break;
		case CAUSE.ENTITY_REMOVED:
			perform = new DelegatePerformMethod(actionWhenEntityRemoved);
			break;
		}
	}

	private void actionWhenComponentsUpdated(){
		foreach (Family family in FamilyManager._families.Values) {
			if(family.matches(_entity))
				family._entitiesIds.Add(_entityId);
			else
				family._entitiesIds.Remove(_entityId);
		}
	}

	private void actionWhenEntityAdded(){
		foreach(Family family in FamilyManager._families.Values)
			if(family.matches(_entity))
				family._entitiesIds.Add(_entityId);
	}

	private void actionWhenEntityRemoved(){
		foreach(Family family in FamilyManager._families.Values)
			family._entitiesIds.Remove(_entityId);
	}
}