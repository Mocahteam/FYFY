public class AllOfTypes : ComponentTypeMatcher {
	public AllOfTypes(params System.Type[] componentTypes) : base(componentTypes) {
	}

	internal override bool matches(GameObjectWrapper gameObjectWrapper){
		for (int i = 0; i < _componentTypeIds.Length; ++i)
			if(gameObjectWrapper._componentTypeIds.Contains(_componentTypeIds[i]) == false)
				return false;
		return true;
	}
}