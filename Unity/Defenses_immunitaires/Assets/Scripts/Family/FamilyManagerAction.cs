internal class FamilyManagerAction {
	internal enum CAUSE { COMPONENT_ADDED, COMPONENT_REMOVED, ENTITY_ADDED, ENTITY_REMOVED };

	private readonly int _entityWrapperId;
	private readonly UECS.EntityWrapper _entityWrapper;

	internal delegate void DelegatePerformMethod();
	internal readonly DelegatePerformMethod perform;

	internal FamilyManagerAction(CAUSE cause, int entityWrapperId, UECS.EntityWrapper entityWrapper) {
		_entityWrapperId = entityWrapperId;
		_entityWrapper = entityWrapper;

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
			if(family.matches(_entityWrapper))
				family._entityWrapperIds.Add(_entityWrapperId);
			else
				family._entityWrapperIds.Remove(_entityWrapperId);
		}
	}

	private void actionWhenEntityAdded(){
		foreach(Family family in FamilyManager._families.Values)
			if(family.matches(_entityWrapper))
				family._entityWrapperIds.Add(_entityWrapperId);
	}

	private void actionWhenEntityRemoved(){
		foreach(Family family in FamilyManager._families.Values)
			family._entityWrapperIds.Remove(_entityWrapperId);
	}
}